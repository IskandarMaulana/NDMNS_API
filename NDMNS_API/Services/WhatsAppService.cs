using Microsoft.IdentityModel.Tokens;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;
using System.Text;
using System.Text.Json;

namespace NDMNS_API.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;
        private readonly HelperService _helperService;
        private readonly SmtpService _smtpService;

        private readonly MessageRepository _messageRepository;
        private readonly DowntimeRepository _downtimeRepository;
        private readonly IspRepository _ispRepository;
        private readonly SiteRepository _siteRepository;
        private readonly NetworkRepository _networkRepository;
        private readonly EmailRepository _emailRepository;
        private readonly PicRepository _picRepository;
        private readonly HelpdeskRepository _helpdeskRepository;
        private readonly UserRepository _userRepository;

        public WhatsAppService(
            HttpClient httpClient,
            IConfiguration configuration,
            HelperService helperService
        )
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _helperService = helperService;
            _smtpService = new SmtpService(configuration, helperService);
            _ispRepository = new IspRepository(configuration, helperService);
            _siteRepository = new SiteRepository(configuration, helperService);
            _messageRepository = new MessageRepository(configuration, helperService);
            _emailRepository = new EmailRepository(configuration);
            _networkRepository = new NetworkRepository(configuration);
            _downtimeRepository = new DowntimeRepository(configuration);
            _picRepository = new PicRepository(configuration);
            _helpdeskRepository = new HelpdeskRepository(configuration);
            _userRepository = new UserRepository(configuration);

            _httpClient.BaseAddress = new Uri(
                _configuration["WhatsAppService:BaseUrl"] ?? "http://localhost:3000"
            );
        }

        private string FormatDateTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "N/A";

            var dt = dateTime.Value;
            var monthNames = new string[]
            {
                "",
                "Januari",
                "Februari",
                "Maret",
                "April",
                "Mei",
                "Juni",
                "Juli",
                "Agustus",
                "September",
                "Oktober",
                "November",
                "Desember",
            };

            return $"{dt.Day:D2} {monthNames[dt.Month]} {dt.Year} pukul {dt:HH:mm}";
        }

        private string FormatInterval(int milliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            string downSpan = "";

            if (timeSpan.Days > 0)
            {
                downSpan += $"{timeSpan.Days} hari ";
            }

            if (timeSpan.Hours > 0)
            {
                downSpan += $"{timeSpan.Hours} jam ";
            }

            if (timeSpan.Minutes > 0)
            {
                downSpan += $"{timeSpan.Minutes} menit ";
            }

            if (timeSpan.Seconds > 0)
            {
                downSpan += $"{timeSpan.Seconds} detik";
            }

            downSpan = downSpan.Trim();
            return downSpan;
        }

        public async Task<QrCodeResponse> GetQrCodeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/whatsapp/qr");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<QrCodeResponse>()
                    ?? new QrCodeResponse { Status = "Error retrieving QR code" };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new QrCodeResponse
                {
                    IsReady = false,
                    Status = $"Error: Cannot connect to WhatsApp Service",
                };
            }
        }

        public DtoResponse<UserViewModel> UpdateStatus(string whatsAppNumber, string status)
        {
            try
            {
                if (
                    !whatsAppNumber.IsNullOrEmpty()
                    && (status.Equals("connected") || status.Equals("disconnected"))
                )
                {
                    DtoResponse<UserViewModel> dto = _userRepository.GetUserByNumber(
                        whatsAppNumber
                    );
                    if (!dto.status.Equals(200) || dto.data == null)
                    {
                        return new DtoResponse<UserViewModel>
                        {
                            status = 500,
                            message = dto.message,
                            data = null,
                        };
                    }

                    UserViewModel user = dto.data;

                    if (status.Equals("connected") && !user.WhatsAppClient.Equals(2))
                    {
                        user.WhatsAppClient = 2;
                    }
                    else if (status.Equals("disconnected") && !user.WhatsAppClient.Equals(1))
                    {
                        user.WhatsAppClient = 1;
                    }
                    else
                    {
                        return new DtoResponse<UserViewModel>
                        {
                            status = 500,
                            message = "Error: Status not valid",
                            data = null,
                        };
                    }

                    DtoResponse<UserViewModel> dtoUpdate = _userRepository.UpdateStatusUser(
                        user.Id,
                        user
                    );

                    if (!dtoUpdate.status.Equals(200) || dtoUpdate.data == null)
                    {
                        return new DtoResponse<UserViewModel>
                        {
                            status = 500,
                            message = dtoUpdate.message,
                            data = null,
                        };
                    }

                    return dtoUpdate;
                }
                else
                {
                    return new DtoResponse<UserViewModel>
                    {
                        status = 400,
                        message = $"Error: Status unknown",
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new DtoResponse<UserViewModel>
                {
                    status = 400,
                    message = $"Error: Update whatsapp client status failed",
                };
            }
        }

        public async Task<DtoResponse<List<GroupResponse>>> GetGroupsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/whatsapp/groups");
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<
                    DtoResponse<List<GroupResponse>>
                >();

                if (result == null)
                {
                    return new DtoResponse<List<GroupResponse>>
                    {
                        status = 400,
                        message = "Error retreiving Groups",
                    };
                }

                if (result.data != null)
                {
                    result.data = result.data.OrderBy(group => group.Name).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                return new DtoResponse<List<GroupResponse>>
                {
                    status = 500,
                    message = $"Internal Server Error: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// Send Message Downtime when there is a network status Down or Intermittent detected. Method status: Done
        /// </summary>
        /// <param name="req"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<DtoResponse<WhatsAppMessage>> SendMessageDowntime(
            SendRequest req,
            string userId
        )
        {
            try
            {
                DtoResponse<NetworkViewModel> dtoNetworkById = _networkRepository.GetNetworkById(
                    req.NetworkId
                );

                if (!dtoNetworkById.status.Equals(200) || dtoNetworkById.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoNetworkById.message,
                        data = null,
                    };
                }

                NetworkViewModel net = dtoNetworkById.data;

                if (!net.Status.Equals(1) && !net.Status.Equals(3))
                {
                    Console.WriteLine(net.Status);
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = "This Network status is not Down or Intermittent",
                        data = null,
                    };
                }

                DtoResponse<string> dtoTicketNumber =
                    await _helperService.GenerateTicketNumberAsync(net.SiteId);

                if (!dtoTicketNumber.status.Equals(200) || dtoTicketNumber.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = "Error generating ticket number: " + dtoTicketNumber.message,
                        data = null,
                    };
                }

                string ticketNumber = dtoTicketNumber.data;

                Downtime downtime = new Downtime
                {
                    NetworkId = net.Id,
                    Description = net.Name + (net.Status.Equals(1) ? " Down" : " Intermittent"),
                    TicketNumber = ticketNumber,
                    Date = DateTime.Now,
                    Start = net.LastUpdate,
                    Category = 1,
                    Status = 1,
                    CreatedBy = userId,
                };

                DtoResponse<DowntimeViewModel> dtoAddDowntime = _downtimeRepository.AddDowntime(
                    downtime,
                    userId
                );

                if (!dtoAddDowntime.status.Equals(201) || dtoAddDowntime.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoAddDowntime.message,
                        data = null,
                    };
                }

                DtoResponse<SiteViewModel> dtoSiteById = _siteRepository.GetSiteById(net.SiteId);

                if (!dtoSiteById.status.Equals(200) || dtoSiteById.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = "Get Site Error: " + dtoSiteById.message,
                        data = null,
                    };
                }

                SiteViewModel site = dtoSiteById.data;

                string msgBody = "";
                int hour = DateTime.Now.Hour;

                if (hour >= 4 && hour < 11)
                    msgBody = "Selamat Pagi, Pak.";
                else if (hour >= 11 && hour < 15)
                    msgBody = "Selamat Siang, Pak.";
                else if (hour >= 15 && hour < 18)
                    msgBody = "Selamat Sore, Pak.";
                else
                    msgBody = "Selamat Malam, Pak.";

                DtoResponse<int> dtoPing = await _helperService.GetTotalPingTime();

                if (!dtoPing.status.Equals(200) || dtoPing.data == 0)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = "Get Setting Error: " + dtoPing.message,
                        data = null,
                    };
                }

                int pingInterval = dtoPing.data;

                msgBody +=
                    $"\nTerpantau link berikut:"
                    + $"\nLink: *{net.Name}*"
                    + $"\nStatus: *{(net.Status == 1 ? "Down" : "Intermittent")}*"
                    + $"\nWaktu: *{FormatDateTime(downtime.Start)} WIB*"
                    + $"\nNo Tiket: *{ticketNumber}*"
                    + $"\nPing Interval: *{FormatInterval(pingInterval)}*"
                    + $"\nApakah ada kendala/maintenance internal? Terimakasih";

                WhatsAppRequest whatsAppRequest = new WhatsAppRequest
                {
                    To = site.WhatsappGroup,
                    Message = msgBody,
                    SendType = "group",
                    MessageType = "media",
                    Contents = new MessageContent
                    {
                        MessageMedia = new MessageMedia
                        {
                            IsBase64 = true,
                            Media = req.Image,
                            MimeType = "image/png",
                            Filename = "image.jpg",
                        },
                    },
                    Options = null,
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(whatsAppRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("/api/whatsapp/send", content);

                var dtoSend = await response.Content.ReadFromJsonAsync<
                    DtoResponse<WhatsAppMessage>
                >();

                if (dtoSend == null)
                {
                    DowntimeViewModel dtmError = dtoAddDowntime.data;

                    var messageError = new Message
                    {
                        Date = DateTime.Now,
                        Recipient = site.WhatsappGroupName,
                        RecipientType = 1,
                        MessageId = "-",
                        Text = msgBody,
                        Image = req.Image,
                        Type = 1,
                        Category = 1,
                        Level = 1,
                        Status = 3,
                        DowntimeId = dtmError.Id,
                    };

                    DtoResponse<MessageViewModel> dtoAddMessageError =
                        _messageRepository.AddMessage(messageError, userId);

                    if (!dtoAddMessageError.status.Equals(201) || dtoAddMessageError.data == null)
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 500,
                            message = dtoAddMessageError.message,
                            data = null,
                        };
                    }

                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = "Internal Server Error: Failed sending message",
                        data = null,
                    };
                }

                if (!dtoSend.status.Equals(200) || dtoSend.data == null)
                {
                    DowntimeViewModel dtmError = dtoAddDowntime.data;

                    var messageError = new Message
                    {
                        Date = DateTime.Now,
                        Recipient = site.WhatsappGroupName,
                        RecipientType = 1,
                        MessageId = "-",
                        Text = msgBody,
                        Image = req.Image,
                        Type = 1,
                        Category = 1,
                        Level = 1,
                        Status = 3,
                        DowntimeId = dtmError.Id,
                    };

                    DtoResponse<MessageViewModel> dtoAddMessageError =
                        _messageRepository.AddMessage(messageError, userId);

                    if (!dtoAddMessageError.status.Equals(201) || dtoAddMessageError.data == null)
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 500,
                            message = dtoAddMessageError.message,
                            data = null,
                        };
                    }

                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoSend.message,
                        data = null,
                    };
                }

                DtoResponse<DowntimeViewModel> dtoLatestDowntime =
                    _downtimeRepository.GetLatestDowntimeByTicketNumber(ticketNumber);

                if (!dtoLatestDowntime.status.Equals(200) || dtoLatestDowntime.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoLatestDowntime.message,
                        data = null,
                    };
                }

                DowntimeViewModel dtm = dtoLatestDowntime.data;

                var message = new Message
                {
                    Date = Convert.ToDateTime(dtoSend.data.Timestamp),
                    Recipient = dtoSend.data.To,
                    RecipientType = 1,
                    MessageId = dtoSend.data.Id,
                    Text = dtoSend.data.Body,
                    Image = req.Image,
                    Type = 1,
                    Category = 1,
                    Level = 1,
                    Status = 1,
                    DowntimeId = dtm.Id,
                };

                DtoResponse<MessageViewModel> dtoAddMessage = _messageRepository.AddMessage(
                    message,
                    userId
                );

                if (!dtoAddMessage.status.Equals(201) || dtoAddMessage.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoAddMessage.message,
                        data = null,
                    };
                }

                DtoResponse<IspViewModel> dtoIspById = _ispRepository.GetIspById(net.IspId);

                if (!dtoIspById.status.Equals(200) || dtoIspById.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = "Get Site Error: " + dtoIspById.message,
                        data = null,
                    };
                }

                IspViewModel isp = dtoIspById.data;

                DtoResponse<EmailViewModel> dtoSendEmail = await SendEmailDowntime(
                    net,
                    downtime,
                    isp,
                    dtoAddMessage.data
                );

                if (!dtoSendEmail.status.Equals(200) || dtoSendEmail.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoSendEmail.message,
                        data = null,
                    };
                }

                return new DtoResponse<WhatsAppMessage>
                {
                    status = 200,
                    message = dtoSend.message,
                    data = dtoSend.data,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<WhatsAppMessage>
                {
                    status = 400,
                    message = $"Error: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// Send Message Uptime when there is a network status Up detected. Method status: On Progress
        /// </summary>
        /// <param name="req"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<DtoResponse<WhatsAppMessage>> SendMessageUptime(
            SendRequest req,
            string userId
        )
        {
            try
            {
                DtoResponse<NetworkViewModel> dtoNetworkById = _networkRepository.GetNetworkById(
                    req.NetworkId
                );

                if (!dtoNetworkById.status.Equals(200) || dtoNetworkById.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoNetworkById.message,
                        data = null,
                    };
                }

                NetworkViewModel net = dtoNetworkById.data;

                DtoResponse<DowntimeViewModel> dtoLatestDowntime =
                    _downtimeRepository.GetLatestDowntimeByNetworkId(net.Id);
                if (!dtoLatestDowntime.status.Equals(200) || dtoLatestDowntime.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoLatestDowntime.message,
                        data = null,
                    };
                }

                DowntimeViewModel downtime = dtoLatestDowntime.data;

                if (
                    net != null
                    && downtime != null
                    && net.Status.Equals(2)
                    && downtime.Status.Equals(1)
                )
                {
                    Downtime dtm = new Downtime
                    {
                        Id = downtime.Id,
                        NetworkId = downtime.NetworkId,
                        Description = downtime.Description,
                        TicketNumber = downtime.TicketNumber,
                        Date = downtime.Date,
                        Start = downtime.Start,
                        Category = downtime.Category,
                        Subcategory = downtime.Subcategory,
                        Action = downtime.Action,
                        Status = 2,
                        End = net.LastUpdate,
                        UpdatedBy = userId,
                    };

                    DtoResponse<DowntimeViewModel> dtoUpdateDowntime =
                        _downtimeRepository.UpdateDowntime(dtm, userId);

                    if (!dtoUpdateDowntime.status.Equals(200) || dtoUpdateDowntime.data == null)
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 500,
                            message = dtoUpdateDowntime.message,
                            data = null,
                        };
                    }

                    DtoResponse<SiteViewModel> dtoSiteById = _siteRepository.GetSiteById(
                        net.SiteId
                    );

                    if (!dtoSiteById.status.Equals(200) || dtoSiteById.data == null)
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 500,
                            message = dtoSiteById.message,
                            data = null,
                        };
                    }

                    SiteViewModel site = dtoSiteById.data;

                    string msgBody = "";
                    int hour = DateTime.Now.Hour;

                    if (hour >= 4 && hour < 11)
                        msgBody = "Selamat Pagi, Pak.";
                    else if (hour >= 11 && hour < 15)
                        msgBody = "Selamat Siang, Pak.";
                    else if (hour >= 15 && hour < 18)
                        msgBody = "Selamat Sore, Pak.";
                    else
                        msgBody = "Selamat Malam, Pak.";

                    DtoResponse<int> dtoPing = await _helperService.GetTotalPingTime();
                    if (!dtoPing.status.Equals(200) || dtoPing.data == 0)
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 500,
                            message = dtoPing.message,
                            data = null,
                        };
                    }
                    int pingInterval = dtoPing.data;

                    if (downtime.Category.Equals(1))
                    {
                        // msgBody =
                        //     "*Update*\n"
                        //     + msgBody
                        //     + $"Terpantau link *{net.Name}* sudah kembali *Up* sejak *{FormatDateTime(dtm.End)} WIB*.\nMohon konfirmasi apakah perbaikan sudah selesai dilakukan?";
                        // + $"\nAction: "

                        msgBody =
                            "*Update*\n"
                            + msgBody
                            + $"Terpantau link berikut:"
                            + $"\nLink: *{net.Name}*"
                            + $"\nStatus: *Up*"
                            + $"\nWaktu: *{FormatDateTime(downtime.Start)} WIB*"
                            + $"\nNo Tiket: *{downtime.TicketNumber}*"
                            + $"\nPing Interval: *{FormatInterval(pingInterval)}*"
                            + $"\nMohon konfirmasi apakah perbaikan sudah selesai dilakukan?";

                        WhatsAppRequest whatsAppRequest = new WhatsAppRequest
                        {
                            To = site.WhatsappGroup,
                            Message = msgBody,
                            SendType = "group",
                            MessageType = "media",
                            Contents = new MessageContent
                            {
                                MessageMedia = new MessageMedia
                                {
                                    IsBase64 = true,
                                    Media = req.Image,
                                    MimeType = "image/png",
                                    Filename = "image.jpg",
                                },
                            },
                            Options = null,
                        };

                        var content = new StringContent(
                            JsonSerializer.Serialize(whatsAppRequest),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync("/api/whatsapp/send", content);
                        var dtoSend = await response.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSend == null)
                        {
                            DowntimeViewModel dtmError = dtoUpdateDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = site.WhatsappGroupName,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 2,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message",
                                data = null,
                            };
                        }

                        if (!dtoSend.status.Equals(200) || dtoSend.data == null)
                        {
                            DowntimeViewModel dtmError = dtoUpdateDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = site.WhatsappGroupName,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 2,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSend.message,
                                data = null,
                            };
                        }

                        var message = new Message
                        {
                            Date = Convert.ToDateTime(dtoSend.data.Timestamp),
                            Recipient = dtoSend.data.To,
                            RecipientType = 1,
                            MessageId = dtoSend.data.Id,
                            Text = dtoSend.data.Body,
                            Image = req.Image,
                            Type = 2,
                            Category = 1,
                            Level = 1,
                            Status = 1,
                            DowntimeId = downtime.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessage = _messageRepository.AddMessage(
                            message,
                            userId
                        );

                        if (!dtoAddMessage.status.Equals(201) || dtoAddMessage.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessage.message,
                                data = null,
                            };
                        }

                        DtoResponse<IspViewModel> dtoIspById = _ispRepository.GetIspById(net.IspId);

                        if (!dtoIspById.status.Equals(200) || dtoIspById.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Get ISP Error: " + dtoIspById.message,
                                data = null,
                            };
                        }

                        IspViewModel isp = dtoIspById.data;

                        DtoResponse<EmailViewModel> dtoSendEmail = await SendEmailUptime(
                            net,
                            dtm,
                            isp,
                            dtoAddMessage.data
                        );

                        if (!dtoSendEmail.status.Equals(200) || dtoSendEmail.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSendEmail.message,
                                data = null,
                            };
                        }

                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 200,
                            message = dtoSend.message,
                            data = dtoSend.data,
                        };
                    }
                    else if (downtime.Category.Equals(2))
                    {
                        DtoResponse<IspViewModel> dtoIspById = _ispRepository.GetIspById(net.IspId);

                        if (!dtoIspById.status.Equals(200) || dtoIspById.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoIspById.message,
                                data = null,
                            };
                        }

                        IspViewModel isp = dtoIspById.data;

                        // msgBody +=
                        //     $"Terpantau link *{net.Name}* sudah kembali *Up* sejak *{FormatDateTime(dtm.End)} WIB*.";
                        msgBody +=
                            $"Terpantau link berikut:"
                            + $"\nLink: *{net.Name}*"
                            + $"\nStatus: *Up*"
                            + $"\nWaktu: *{FormatDateTime(downtime.Start)} WIB*"
                            + $"\nNo Tiket: *{downtime.TicketNumber}*"
                            + $"\nPing Interval: *{FormatInterval(pingInterval)}*";

                        WhatsAppRequest whatsAppRequest = new WhatsAppRequest
                        {
                            To = site.WhatsappGroup,
                            Message = msgBody,
                            SendType = "group",
                            MessageType = "media",
                            Contents = new MessageContent
                            {
                                MessageMedia = new MessageMedia
                                {
                                    IsBase64 = true,
                                    Media = req.Image,
                                    MimeType = "image/png",
                                    Filename = "image.jpg",
                                },
                            },
                            Options = null,
                        };

                        var contentSite = new StringContent(
                            JsonSerializer.Serialize(whatsAppRequest),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var responseSite = await _httpClient.PostAsync(
                            "/api/whatsapp/send",
                            contentSite
                        );
                        var dtoSendSite = await responseSite.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSendSite == null)
                        {
                            DowntimeViewModel dtmError = dtoUpdateDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = site.WhatsappGroupName,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 2,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message to Site",
                                data = null,
                            };
                        }

                        if (!dtoSendSite.status.Equals(200) || dtoSendSite.data == null)
                        {
                            DowntimeViewModel dtmError = dtoUpdateDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = site.WhatsappGroupName,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 2,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSendSite.message,
                                data = null,
                            };
                        }

                        var messageSite = new Message
                        {
                            Date = Convert.ToDateTime(dtoSendSite.data.Timestamp),
                            Recipient = dtoSendSite.data.To,
                            RecipientType = 1,
                            MessageId = dtoSendSite.data.Id,
                            Text = dtoSendSite.data.Body,
                            Image = req.Image,
                            Type = 2,
                            Category = 1,
                            Level = 1,
                            Status = 1,
                            DowntimeId = downtime.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessageSite =
                            _messageRepository.AddMessage(messageSite, userId);

                        if (!dtoAddMessageSite.status.Equals(201) || dtoAddMessageSite.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessageSite.message,
                                data = null,
                            };
                        }

                        msgBody += "\nMohon dapat diinfokan detail RFO dari ticket ini.";
                        whatsAppRequest.Message = msgBody;
                        whatsAppRequest.To = isp.WhatsappGroup;

                        var contentIsp = new StringContent(
                            JsonSerializer.Serialize(whatsAppRequest),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var responseIsp = await _httpClient.PostAsync(
                            "/api/whatsapp/send",
                            contentIsp
                        );
                        var dtoSendIsp = await responseIsp.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSendIsp == null)
                        {
                            DowntimeViewModel dtmError = dtoUpdateDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = isp.WhatsappGroup,
                                RecipientType = 2,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 2,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message to Site",
                                data = null,
                            };
                        }

                        if (!dtoSendIsp.status.Equals(200) || dtoSendIsp.data == null)
                        {
                            DowntimeViewModel dtmError = dtoUpdateDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = isp.WhatsappGroup,
                                RecipientType = 2,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 2,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSendIsp.message,
                                data = null,
                            };
                        }

                        var messageIsp = new Message
                        {
                            Date = Convert.ToDateTime(dtoSendIsp.data.Timestamp),
                            Recipient = dtoSendIsp.data.To,
                            RecipientType = 2,
                            MessageId = dtoSendIsp.data.Id,
                            Text = dtoSendIsp.data.Body,
                            Image = req.Image,
                            Type = 2,
                            Category = 1,
                            Level = 1,
                            Status = 1,
                            DowntimeId = downtime.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessageIsp =
                            _messageRepository.AddMessage(messageIsp, userId);

                        if (!dtoAddMessageIsp.status.Equals(201) || dtoAddMessageIsp.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessageIsp.message,
                                data = null,
                            };
                        }

                        DtoResponse<EmailViewModel> dtoSendEmail = await SendEmailUptime(
                            net,
                            dtm,
                            isp,
                            dtoAddMessageIsp.data
                        );

                        if (!dtoSendEmail.status.Equals(200) || dtoSendEmail.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSendEmail.message,
                                data = null,
                            };
                        }

                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 200,
                            message = dtoSendIsp.message,
                            data = dtoSendIsp.data,
                        };
                    }
                    else
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 400,
                            message = "Failed: This downtime has no Category",
                            data = null,
                        };
                    }
                }
                else
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 400,
                        message = "Failed: This Network and Downtime data are not valid",
                        data = null,
                    };
                }
            }
            catch (Exception ex)
            {
                return new DtoResponse<WhatsAppMessage>
                {
                    status = 400,
                    message = $"Error: {ex.Message}",
                    data = null,
                };
            }
        }

        /// <summary>
        /// Send Message Update when there is a network status Down or Intermittent for more than 1 hour. Method status: On Progress
        /// </summary>
        /// <param name="req"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<DtoResponse<WhatsAppMessage>> SendMessageUpdate(
            SendRequest req,
            string userId
        )
        {
            try
            {
                DtoResponse<NetworkViewModel> dtoNetworkById = _networkRepository.GetNetworkById(
                    req.NetworkId
                );

                if (!dtoNetworkById.status.Equals(200) || dtoNetworkById.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoNetworkById.message,
                        data = null,
                    };
                }

                NetworkViewModel net = dtoNetworkById.data;

                DtoResponse<DowntimeViewModel> dtoLatestDowntime =
                    _downtimeRepository.GetLatestDowntimeByNetworkId(net.Id);
                if (!dtoLatestDowntime.status.Equals(200) || dtoLatestDowntime.data == null)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoLatestDowntime.message,
                        data = null,
                    };
                }

                DowntimeViewModel downtime = dtoLatestDowntime.data;

                TimeSpan timeSpan = DateTime.Now - downtime.Start;

                if (timeSpan.TotalMinutes < 60)
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 400,
                        message =
                            $"Downtime is under 1 hour. Current duration: {Math.Round(timeSpan.TotalMinutes, 2)} minutes",
                    };
                }

                DtoResponse<MessageViewModel> dtoLatestMessageByDowntime =
                    _messageRepository.GetLatestMessageByDowntimeId(downtime.Id);
                if (
                    !dtoLatestMessageByDowntime.status.Equals(200)
                    || dtoLatestMessageByDowntime.data == null
                )
                {
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoLatestMessageByDowntime.message,
                        data = null,
                    };
                }

                MessageViewModel msg = dtoLatestMessageByDowntime.data;

                if (
                    net != null
                    && downtime != null
                    && net.Status.Equals(1)
                    && downtime.Status.Equals(1)
                    && timeSpan.TotalHours > msg.Level - 1
                )
                {
                    DtoResponse<int> dtoPing = await _helperService.GetTotalPingTime();
                    if (!dtoPing.status.Equals(200) || dtoPing.data == 0)
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 500,
                            message = dtoPing.message,
                            data = null,
                        };
                    }

                    int pingInterval = dtoPing.data;

                    string msgBody = "";

                    string downSpan = "";

                    if (timeSpan.Days > 0)
                    {
                        downSpan += $"{timeSpan.Days} hari ";
                    }

                    if (timeSpan.Hours > 0)
                    {
                        downSpan += $"{timeSpan.Hours} jam ";
                    }

                    if (timeSpan.Minutes > 0)
                    {
                        downSpan += $"{timeSpan.Minutes} menit";
                    }

                    downSpan = downSpan.Trim();

                    int hour = DateTime.Now.Hour;

                    if (hour >= 4 && hour < 11)
                        msgBody = "Selamat Pagi, ";
                    else if (hour >= 11 && hour < 15)
                        msgBody = "Selamat Siang, ";
                    else if (hour >= 15 && hour < 18)
                        msgBody = "Selamat Sore, ";
                    else
                        msgBody = "Selamat Malam, ";

                    // msgBody +=
                    //     $"Terpantau link *{net.Name}* sudah dalam kondisi *Down* selama *{downSpan}* sejak *{FormatDateTime(downtime.Start)} WIB*.\nMohon dapat diinfokan apakah ada update progress perbaikan?";
                    msgBody +=
                        $"\nTerpantau link berikut sudah dalam kondisi *{(net.Status == 1 ? "Down" : "Intermittent")}* selama *{downSpan}* :"
                        + $"\nLink: *{net.Name}*"
                        + $"\nStatus: *{(net.Status == 1 ? "Down" : "Intermittent")}*"
                        + $"\nWaktu: *{FormatDateTime(downtime.Start)} WIB*"
                        + $"\nNo Tiket: *{downtime.TicketNumber}*"
                        + $"\nPing Interval: *{FormatInterval(pingInterval)}*"
                        + $"\nMohon dapat diinfokan apakah ada update progress perbaikan? Terimakasih";

                    if (downtime.Category.Equals(1))
                    {
                        DtoResponse<SiteViewModel> dtoSiteById = _siteRepository.GetSiteById(
                            net.SiteId
                        );

                        if (!dtoSiteById.status.Equals(200) || dtoSiteById.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSiteById.message,
                                data = null,
                            };
                        }

                        SiteViewModel site = dtoSiteById.data;

                        DtoResponse<MessageViewModel> dtoFirstMessageByDowntime =
                            _messageRepository.GetFirstMessageByDowntimeId(
                                downtime.Id,
                                site.WhatsappGroup
                            );
                        if (
                            !dtoFirstMessageByDowntime.status.Equals(200)
                            || dtoFirstMessageByDowntime.data == null
                        )
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoFirstMessageByDowntime.message,
                                data = null,
                            };
                        }

                        MessageViewModel msgReply = dtoFirstMessageByDowntime.data;

                        WhatsAppRequest whatsAppRequest = new WhatsAppRequest
                        {
                            To = site.WhatsappGroup,
                            Message = msgBody,
                            SendType = "group",
                            MessageType = "text",
                            // Options = new MessageOption { QuotedMessageId = msgReply.MessageId },
                        };

                        var content = new StringContent(
                            JsonSerializer.Serialize(whatsAppRequest),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync("/api/whatsapp/send", content);
                        var dtoSend = await response.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSend == null)
                        {
                            DowntimeViewModel dtmError = dtoLatestDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = site.WhatsappGroup,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 1,
                                Category = 3,
                                Level = msg.Level + 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message",
                                data = null,
                            };
                        }

                        if (!dtoSend.status.Equals(200) || dtoSend.data == null)
                        {
                            DowntimeViewModel dtmError = dtoLatestDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = site.WhatsappGroup,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 1,
                                Category = 3,
                                Level = msg.Level + 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSend.message,
                                data = null,
                            };
                        }

                        var message = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Date = Convert.ToDateTime(dtoSend.data.Timestamp),
                            Recipient = dtoSend.data.To,
                            RecipientType = 1,
                            MessageId = dtoSend.data.Id,
                            Text = dtoSend.data.Body,
                            Type = 1,
                            Category = 3,
                            Level = msg.Level + 1,
                            Status = 1,
                            DowntimeId = downtime.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessage = _messageRepository.AddMessage(
                            message,
                            userId
                        );

                        if (!dtoAddMessage.status.Equals(201) || dtoAddMessage.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessage.message,
                                data = null,
                            };
                        }

                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 200,
                            message = dtoSend.message,
                            data = dtoSend.data,
                        };
                    }
                    else if (downtime.Category.Equals(2))
                    {
                        DtoResponse<IspViewModel> dtoIspById = _ispRepository.GetIspById(net.IspId);

                        if (!dtoIspById.status.Equals(200) || dtoIspById.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoIspById.message,
                                data = null,
                            };
                        }

                        IspViewModel isp = dtoIspById.data;

                        DtoResponse<MessageViewModel> dtoFirstMessageByDowntime =
                            _messageRepository.GetFirstMessageByDowntimeId(
                                downtime.Id,
                                isp.WhatsappGroup
                            );
                        if (
                            !dtoFirstMessageByDowntime.status.Equals(200)
                            || dtoFirstMessageByDowntime.data == null
                        )
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoFirstMessageByDowntime.message,
                                data = null,
                            };
                        }

                        MessageViewModel msgReply = dtoFirstMessageByDowntime.data;

                        WhatsAppRequest whatsAppRequest = new WhatsAppRequest
                        {
                            To = isp.WhatsappGroup,
                            Message = msgBody,
                            SendType = "group",
                            MessageType = "text",
                            // Options = new MessageOption { QuotedMessageId = msgReply.MessageId },
                        };

                        var content = new StringContent(
                            JsonSerializer.Serialize(whatsAppRequest),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync("/api/whatsapp/send", content);
                        var dtoSend = await response.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSend == null)
                        {
                            DowntimeViewModel dtmError = dtoLatestDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = isp.WhatsappGroup,
                                RecipientType = 2,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 1,
                                Category = 3,
                                Level = msg.Level + 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message to ISP",
                                data = null,
                            };
                        }

                        if (!dtoSend.status.Equals(200) || dtoSend.data == null)
                        {
                            DowntimeViewModel dtmError = dtoLatestDowntime.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = isp.WhatsappGroup,
                                RecipientType = 2,
                                MessageId = "-",
                                Text = msgBody,
                                Image = req.Image,
                                Type = 1,
                                Category = 3,
                                Level = msg.Level + 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, userId);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSend.message,
                                data = null,
                            };
                        }

                        var message = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Date = Convert.ToDateTime(dtoSend.data.Timestamp),
                            Recipient = dtoSend.data.To,
                            RecipientType = 2,
                            MessageId = dtoSend.data.Id,
                            Text = dtoSend.data.Body,
                            Type = 1,
                            Category = 3,
                            Level = msg.Level + 1,
                            Status = 1,
                            DowntimeId = downtime.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessage = _messageRepository.AddMessage(
                            message,
                            userId
                        );

                        if (!dtoAddMessage.status.Equals(201) || dtoAddMessage.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessage.message,
                                data = null,
                            };
                        }

                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 200,
                            message = dtoSend.message,
                            data = dtoSend.data,
                        };
                    }
                    else
                    {
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 400,
                            message = "Failed",
                        };
                    }
                }
                else
                {
                    return new DtoResponse<WhatsAppMessage> { status = 400, message = "Failed" };
                }
            }
            catch (Exception ex)
            {
                return new DtoResponse<WhatsAppMessage>
                {
                    status = 400,
                    message = $"Error: {ex.Message}",
                };
            }
        }

        /// <summary>
        /// Downtime Follow Up when there is a message received form WhatsApp Web Client. Method status: On Progress
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<DtoResponse<WhatsAppMessage>> DowntimeFollowUp(WhatsAppMessage message)
        {
            try
            {
                DtoResponse<MessageViewModel> dtoMsgByWhatsApp =
                    _messageRepository.GetMessageByMessageId(message.RepliedToMessageId);

                if (!dtoMsgByWhatsApp.status.Equals(200) || dtoMsgByWhatsApp.data == null)
                {
                    Console.WriteLine("dtoMsgByWhatsApp null");

                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoMsgByWhatsApp.message,
                        data = null,
                    };
                }

                MessageViewModel msg = dtoMsgByWhatsApp.data;
                DtoResponse<Downtime> dtoDtmByMsg = _downtimeRepository.GetDowntimeByIdCrea(
                    msg.DowntimeId
                );

                if (!dtoDtmByMsg.status.Equals(200) || dtoDtmByMsg.data == null)
                {
                    Console.WriteLine("dtoDtmByMsg null");
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = dtoDtmByMsg.message,
                        data = null,
                    };
                }

                Downtime dtm = dtoDtmByMsg.data;

                if (
                    dtm != null
                    && msg != null
                    && dtm.Status.Equals(1)
                    && msg.Type.Equals(1)
                    && msg.Category.Equals(1)
                    && msg.Level.Equals(1)
                    && msg.Status.Equals(1)
                    && msg.RecipientType.Equals(1)
                    && (
                        message.Body.Contains("Ya", StringComparison.OrdinalIgnoreCase)
                        || message.Body.Contains("Tidak", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    if (message.Body.Contains("Ya", StringComparison.OrdinalIgnoreCase))
                    {
                        WhatsAppRequest whatsApp = new WhatsAppRequest
                        {
                            To = msg.Recipient,
                            Message = "Baik, Pak mohon update berkala",
                            SendType = "group",
                            MessageType = "text",
                            Contents = null,
                            Options = null,
                        };

                        var content = new StringContent(
                            JsonSerializer.Serialize(whatsApp),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync("/api/whatsapp/send", content);

                        var dtoSend = await response.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSend == null)
                        {
                            Downtime dtmError = dtoDtmByMsg.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = msg.Recipient,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = whatsApp.Message,
                                Image = null,
                                Type = 1,
                                Category = 2,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, dtm.CreatedBy);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message",
                                data = null,
                            };
                        }

                        if (!dtoSend.status.Equals(200) || dtoSend.data == null)
                        {
                            Downtime dtmError = dtoDtmByMsg.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = msg.Recipient,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = whatsApp.Message,
                                Image = null,
                                Type = 1,
                                Category = 2,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, dtm.CreatedBy);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSend.message,
                                data = null,
                            };
                        }

                        var msgFollowUp = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Date = dtoSend.data.Timestamp,
                            Recipient = dtoSend.data.To,
                            RecipientType = 1,
                            MessageId = dtoSend.data.Id,
                            Text = dtoSend.data.Body,
                            Image = null,
                            Type = 1,
                            Category = 2,
                            Level = 2,
                            Status = 1,
                            DowntimeId = dtm.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessage = _messageRepository.AddMessage(
                            msgFollowUp,
                            dtm.CreatedBy
                        );

                        if (!dtoAddMessage.status.Equals(201) || dtoAddMessage.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessage.message,
                                data = null,
                            };
                        }

                        msg.Status = 2;
                        var msgUpdate = new Message
                        {
                            Id = msg.Id,
                            Date = msg.Date,
                            Recipient = msg.Recipient,
                            RecipientType = msg.RecipientType,
                            MessageId = msg.MessageId,
                            Text = msg.Text,
                            Image = msg.Image,
                            Type = msg.Type,
                            Category = msg.Category,
                            Level = msg.Level,
                            Status = msg.Status,
                            DowntimeId = msg.DowntimeId,
                        };

                        DtoResponse<MessageViewModel> dtoUpdateMessage =
                            _messageRepository.UpdateMessage(msg.Id, msgUpdate, dtm.CreatedBy);

                        if (!dtoUpdateMessage.status.Equals(200) || dtoUpdateMessage.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoUpdateMessage.message,
                                data = null,
                            };
                        }

                        dtm.Category = 1;
                        _downtimeRepository.UpdateDowntimeCategory(dtm, dtm.CreatedBy);
                        DtoResponse<DowntimeViewModel> dtoUpdateDowntime =
                            _downtimeRepository.UpdateDowntimeCategory(dtm, dtm.CreatedBy);

                        if (!dtoUpdateDowntime.status.Equals(200) || dtoUpdateDowntime.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoUpdateDowntime.message,
                                data = null,
                            };
                        }

                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 200,
                            message = dtoSend.message,
                            data = dtoSend.data,
                        };
                    }
                    else if (message.Body.Contains("Tidak", StringComparison.OrdinalIgnoreCase))
                    {
                        WhatsAppRequest whatsApp = new WhatsAppRequest
                        {
                            To = msg.Recipient,
                            Message = "Baik, Pak akan kami follow up ke ISP",
                            SendType = "group",
                            MessageType = "text",
                            Contents = null,
                            Options = null,
                        };

                        var content = new StringContent(
                            JsonSerializer.Serialize(whatsApp),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var response = await _httpClient.PostAsync("/api/whatsapp/send", content);
                        var dtoSendSite = await response.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSendSite == null)
                        {
                            Downtime dtmError = dtoDtmByMsg.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = msg.Recipient,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = whatsApp.Message,
                                Image = null,
                                Type = 1,
                                Category = 2,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, dtm.CreatedBy);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message",
                                data = null,
                            };
                        }

                        if (!dtoSendSite.status.Equals(200) || dtoSendSite.data == null)
                        {
                            Downtime dtmError = dtoDtmByMsg.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = msg.Recipient,
                                RecipientType = 1,
                                MessageId = "-",
                                Text = whatsApp.Message,
                                Image = null,
                                Type = 1,
                                Category = 2,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, dtm.CreatedBy);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSendSite.message,
                                data = null,
                            };
                        }

                        var msgFollowUp = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Date = dtoSendSite.data.Timestamp,
                            Recipient = dtoSendSite.data.To,
                            RecipientType = 1,
                            MessageId = dtoSendSite.data.Id,
                            Text = dtoSendSite.data.Body,
                            Image = null,
                            Type = 1,
                            Category = 2,
                            Level = 1,
                            Status = 1,
                            DowntimeId = dtm.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessageSite =
                            _messageRepository.AddMessage(msgFollowUp, dtm.CreatedBy);

                        if (!dtoAddMessageSite.status.Equals(201) || dtoAddMessageSite.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessageSite.message,
                                data = null,
                            };
                        }

                        msg.Status = 2;
                        var msgUpdate = new Message
                        {
                            Id = msg.Id,
                            Date = msg.Date,
                            Recipient = msg.Recipient,
                            RecipientType = msg.RecipientType,
                            MessageId = msg.MessageId,
                            Text = msg.Text,
                            Image = msg.Image,
                            Type = msg.Type,
                            Category = msg.Category,
                            Level = msg.Level,
                            Status = msg.Status,
                            DowntimeId = msg.DowntimeId,
                        };

                        DtoResponse<MessageViewModel> dtoUpdateMessage =
                            _messageRepository.UpdateMessage(msg.Id, msgUpdate, dtm.CreatedBy);

                        if (!dtoUpdateMessage.status.Equals(200) || dtoUpdateMessage.data == null)
                        {
                            Console.WriteLine(dtoUpdateMessage.message);
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoUpdateMessage.message,
                                data = null,
                            };
                        }

                        dtm.Category = 2;
                        _downtimeRepository.UpdateDowntimeCategory(dtm, dtm.CreatedBy);
                        DtoResponse<DowntimeViewModel> dtoUpdateDowntime =
                            _downtimeRepository.UpdateDowntimeCategory(dtm, dtm.CreatedBy);

                        if (!dtoUpdateDowntime.status.Equals(200) || dtoUpdateDowntime.data == null)
                        {
                            Console.WriteLine(dtoUpdateMessage.message);
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoUpdateDowntime.message,
                                data = null,
                            };
                        }

                        DtoResponse<IspViewModel> dtoIspByNetwork =
                            _ispRepository.GetIspByNetworkId(dtm.NetworkId);

                        if (!dtoIspByNetwork.status.Equals(200) || dtoIspByNetwork.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoIspByNetwork.message,
                                data = null,
                            };
                        }

                        IspViewModel isp = dtoIspByNetwork.data;

                        DtoResponse<NetworkViewModel> dtoNetworkById =
                            _networkRepository.GetNetworkById(dtm.NetworkId);

                        if (!dtoNetworkById.status.Equals(200) || dtoNetworkById.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoNetworkById.message,
                                data = null,
                            };
                        }

                        NetworkViewModel net = dtoNetworkById.data;

                        string msgBody = "";
                        int hour = DateTime.Now.Hour;

                        if (hour >= 4 && hour < 11)
                            msgBody = "Selamat Pagi, ";
                        else if (hour >= 11 && hour < 15)
                            msgBody = "Selamat Siang, ";
                        else if (hour >= 15 && hour < 18)
                            msgBody = "Selamat Sore, ";
                        else
                            msgBody = "Selamat Malam, ";

                        DtoResponse<List<HelpdeskViewModel>> dtoHelpdesksByIsp =
                            _helpdeskRepository.GetHelpdesksByIspId(net.IspId);

                        if (!dtoHelpdesksByIsp.status.Equals(200) || dtoHelpdesksByIsp.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoHelpdesksByIsp.message,
                                data = null,
                            };
                        }

                        List<HelpdeskViewModel> helpdesks = dtoHelpdesksByIsp.data;

                        List<string> mentions = new List<string>();

                        foreach (var helpdesk in helpdesks)
                        {
                            if (helpdesk.Role.Equals(1))
                            {
                                msgBody += "@" + helpdesk.WhatsappNumber + " ";
                                mentions.Add(helpdesk.WhatsappNumber + "@c.us");
                            }
                        }

                        DtoResponse<int> dtoPing = await _helperService.GetTotalPingTime();
                        if (!dtoPing.status.Equals(200) || dtoPing.data == 0)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoPing.message,
                                data = null,
                            };
                        }

                        int pingInterval = dtoPing.data;

                        // msgBody +=
                        //     $"Terpantau link *{net.Name}* dalam kondisi *Down* sejak *{FormatDateTime(dtm.Start)} WIB*.\nMohon dapat dibantu untuk dilakukan pengecekan.";
                        msgBody +=
                            $"\nTerpantau link berikut:"
                            + $"\nLink: *{net.Name}*"
                            + $"\nStatus: *{(net.Status == 1 ? "Down" : "Intermittent")}*"
                            + $"\nWaktu: *{FormatDateTime(dtm.Start)} WIB*"
                            + $"\nNo Tiket: *{dtm.TicketNumber}*"
                            + $"\nPing Interval: *{FormatInterval(pingInterval)}*"
                            + $"\nMohon dapat dibantu untuk dilakukan pengecekan. Terimakasih";

                        WhatsAppRequest wa = new WhatsAppRequest
                        {
                            To = isp.WhatsappGroup,
                            Message = msgBody,
                            SendType = "group",
                            MessageType = "media",
                            Contents = new MessageContent
                            {
                                MessageMedia = new MessageMedia
                                {
                                    IsBase64 = true,
                                    Media = msg.Image,
                                    MimeType = "image/png",
                                    Filename = "image.jpg",
                                },
                            },
                            Options = new MessageOption { Mentions = mentions.ToArray() },
                        };

                        var con = new StringContent(
                            JsonSerializer.Serialize(wa),
                            Encoding.UTF8,
                            "application/json"
                        );

                        var res = await _httpClient.PostAsync("/api/whatsapp/send", con);
                        var dtoSendIsp = await res.Content.ReadFromJsonAsync<
                            DtoResponse<WhatsAppMessage>
                        >();

                        if (dtoSendIsp == null)
                        {
                            Downtime dtmError = dtoDtmByMsg.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = isp.WhatsappGroup,
                                RecipientType = 2,
                                MessageId = "-",
                                Text = whatsApp.Message,
                                Image = null,
                                Type = 1,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, dtm.CreatedBy);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = "Internal Server Error: Failed sending message",
                                data = null,
                            };
                        }

                        if (!dtoSendIsp.status.Equals(200) || dtoSendIsp.data == null)
                        {
                            Downtime dtmError = dtoDtmByMsg.data;

                            var messageError = new Message
                            {
                                Date = DateTime.Now,
                                Recipient = isp.WhatsappGroup,
                                RecipientType = 2,
                                MessageId = "-",
                                Text = whatsApp.Message,
                                Image = null,
                                Type = 1,
                                Category = 1,
                                Level = 1,
                                Status = 3,
                                DowntimeId = dtmError.Id,
                            };

                            DtoResponse<MessageViewModel> dtoAddMessageError =
                                _messageRepository.AddMessage(messageError, dtm.CreatedBy);

                            if (
                                !dtoAddMessageError.status.Equals(201)
                                || dtoAddMessageError.data == null
                            )
                            {
                                return new DtoResponse<WhatsAppMessage>
                                {
                                    status = 500,
                                    message = dtoAddMessageError.message,
                                    data = null,
                                };
                            }

                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoSendIsp.message,
                                data = null,
                            };
                        }

                        var msgIsp = new Message
                        {
                            Id = Guid.NewGuid().ToString(),
                            Date = dtoSendIsp.data.Timestamp,
                            Recipient = dtoSendIsp.data.To,
                            RecipientType = 2,
                            MessageId = dtoSendIsp.data.Id,
                            Text = dtoSendIsp.data.Body,
                            Image = msg.Image,
                            Type = 1,
                            Category = 1,
                            Level = 1,
                            Status = 1,
                            DowntimeId = dtm.Id,
                        };

                        DtoResponse<MessageViewModel> dtoAddMessageIsp =
                            _messageRepository.AddMessage(msgIsp, dtm.CreatedBy);

                        if (!dtoAddMessageIsp.status.Equals(201) || dtoAddMessageIsp.data == null)
                        {
                            return new DtoResponse<WhatsAppMessage>
                            {
                                status = 500,
                                message = dtoAddMessageIsp.message,
                                data = null,
                            };
                        }

                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 200,
                            message = dtoSendIsp.message,
                            data = dtoSendIsp.data,
                        };
                    }
                    else
                    {
                        Console.WriteLine("Message body not match");
                        return new DtoResponse<WhatsAppMessage>
                        {
                            status = 400,
                            message = "Message body not match",
                        };
                    }
                }
                else
                {
                    Console.WriteLine(
                        "Message or Downtime not found or Level and Status not match"
                    );
                    return new DtoResponse<WhatsAppMessage>
                    {
                        status = 400,
                        message = "Message or Downtime not found or Level and Status not match",
                    };
                }
            }
            catch (Exception ex)
            {
                return new DtoResponse<WhatsAppMessage>
                {
                    status = 400,
                    message = $"Error: {ex.Message}",
                };
            }
        }

        public async Task<DtoResponse<EmailViewModel>> SendEmailDowntime(
            NetworkViewModel net,
            Downtime dtm,
            IspViewModel isp,
            MessageViewModel msg
        )
        {
            try
            {
                DtoResponse<List<PicViewModel>> dtoPicBySite = _picRepository.GetPicsBySiteId(
                    net.SiteId
                );

                if (!dtoPicBySite.status.Equals(200) || dtoPicBySite.data == null)
                {
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoPicBySite.message,
                        data = null,
                    };
                }

                List<PicViewModel> pics = dtoPicBySite.data;

                var formattedPics = new List<string>();

                foreach (var pic in pics)
                {
                    if (pic.SiteId == net.SiteId)
                    {
                        if (pic.Name.Equals("NOC Pama"))
                        {
                            formattedPics.Add($"{pic.Name} {pic.WhatsappNumber}");
                        }
                        else
                        {
                            formattedPics.Add($"Bpk. {pic.Name} {pic.WhatsappNumber}");
                        }
                    }
                }

                string picNames = "";
                int count = formattedPics.Count;

                if (count == 1)
                {
                    picNames = formattedPics[0];
                }
                else if (count == 2)
                {
                    picNames = string.Join(" & ", formattedPics);
                }
                else if (count > 2)
                {
                    picNames =
                        string.Join(", ", formattedPics.Take(count - 1))
                        + " & "
                        + formattedPics.Last();
                }

                string statusText = net.Status switch
                {
                    1 => "Down",
                    2 => "Up",
                    3 => "Intermittent",
                    _ => "Unknown",
                };

                string emailBody =
                    $@"
                                <html>
                                <body>
                                    <p style=""font-size: 14px;"">Dear Support,</p>
                                    <br>
                                    <p style=""font-size: 14px;"">Mohon bantuannya untuk cek link kami <strong>{net.Name}</strong> terpantau <strong>{statusText}</strong> sejak <strong>{dtm.Start}</strong>:</p>
                                    <p style=""font-size: 14px;"">IP: {net.Ip}</p>
                                    <p style=""font-size: 14px;"">CID: {net.Cid}</p>
                                    <p style=""font-size: 14px;"">PIC: {picNames}</p>
                                    <br>
                                </body>
                                </html>";

                List<DetailEmailPicViewModel> detailEmailPics = [];
                List<DetailEmailHelpdeskViewModel> detailEmailHelpdesks = [];

                DtoResponse<List<PicViewModel>> dtoPicByRole = _picRepository.GetPicsByRole(2);

                if (!dtoPicByRole.status.Equals(200) || dtoPicByRole.data == null)
                {
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoPicByRole.message,
                        data = null,
                    };
                }

                List<PicViewModel> headPics = dtoPicByRole.data;

                foreach (PicViewModel pic in headPics)
                {
                    detailEmailPics.Add(
                        new DetailEmailPicViewModel
                        {
                            PicId = pic.Id,
                            Type = 2,
                            EmailAddress = pic.EmailAddress,
                            PicName = pic.Name,
                        }
                    );
                }

                foreach (PicViewModel pic in pics)
                {
                    detailEmailPics.Add(
                        new DetailEmailPicViewModel
                        {
                            PicId = pic.Id,
                            Type = 2,
                            EmailAddress = pic.EmailAddress,
                            PicName = pic.Name,
                        }
                    );
                }

                DtoResponse<string> dtoSendemail = await _smtpService.SendMailToIsp(
                    msg.Image,
                    $"{net.Name} Terpantau Down",
                    emailBody,
                    isp,
                    detailEmailPics,
                    detailEmailHelpdesks
                );

                if (!dtoSendemail.status.Equals(200) || dtoSendemail.data == null)
                {
                    var emailError = new Email
                    {
                        DowntimeId = dtm.Id,
                        Type = 1,
                        Subject = $"{net.Name} Terpantau Down",
                        Body = emailBody,
                        Image = msg.Image,
                        Date = DateTime.Now,
                        Status = 2,
                    };

                    List<DetailEmailPic> dtlEmailPicsError = [];
                    List<DetailEmailHelpdesk> dtlEmailHelpdesksError = [];
                    foreach (DetailEmailPicViewModel detailEmailPic in detailEmailPics)
                    {
                        dtlEmailPicsError.Add(
                            new DetailEmailPic
                            {
                                PicId = detailEmailPic.PicId,
                                Type = detailEmailPic.Type,
                                EmailAddress = detailEmailPic.EmailAddress,
                            }
                        );
                    }

                    foreach (
                        DetailEmailHelpdeskViewModel detailEmailHelpdesk in detailEmailHelpdesks
                    )
                    {
                        dtlEmailHelpdesksError.Add(
                            new DetailEmailHelpdesk
                            {
                                HelpdeskId = detailEmailHelpdesk.HelpdeskId,
                                Type = detailEmailHelpdesk.Type,
                                EmailAddress = detailEmailHelpdesk.EmailAddress,
                            }
                        );
                    }

                    DtoResponse<EmailViewModel> dtoAddemailError = _emailRepository.AddEmail(
                        emailError,
                        dtlEmailPicsError,
                        dtlEmailHelpdesksError,
                        dtm.CreatedBy
                    );

                    if (!dtoAddemailError.status.Equals(201) || dtoAddemailError.data == null)
                    {
                        return new DtoResponse<EmailViewModel>
                        {
                            status = 500,
                            message = dtoAddemailError.message,
                            data = null,
                        };
                    }

                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoSendemail.message,
                        data = null,
                    };
                }

                var email = new Email
                {
                    DowntimeId = dtm.Id,
                    Type = 1,
                    Subject = $"{net.Name} Terpantau Down",
                    Body = emailBody,
                    Image = msg.Image,
                    Date = DateTime.Now,
                    Status = 1,
                };

                List<DetailEmailPic> dtlEmailPics = [];
                List<DetailEmailHelpdesk> dtlEmailHelpdesks = [];
                foreach (DetailEmailPicViewModel detailEmailPic in detailEmailPics)
                {
                    dtlEmailPics.Add(
                        new DetailEmailPic
                        {
                            PicId = detailEmailPic.PicId,
                            Type = detailEmailPic.Type,
                            EmailAddress = detailEmailPic.EmailAddress,
                        }
                    );
                }

                foreach (DetailEmailHelpdeskViewModel detailEmailHelpdesk in detailEmailHelpdesks)
                {
                    dtlEmailHelpdesks.Add(
                        new DetailEmailHelpdesk
                        {
                            HelpdeskId = detailEmailHelpdesk.HelpdeskId,
                            Type = detailEmailHelpdesk.Type,
                            EmailAddress = detailEmailHelpdesk.EmailAddress,
                        }
                    );
                }

                DtoResponse<EmailViewModel> dtoAddemail = _emailRepository.AddEmail(
                    email,
                    dtlEmailPics,
                    dtlEmailHelpdesks,
                    dtm.CreatedBy
                );

                if (!dtoAddemail.status.Equals(201) || dtoAddemail.data == null)
                {
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoAddemail.message,
                        data = null,
                    };
                }

                return new DtoResponse<EmailViewModel>
                {
                    status = 200,
                    message = dtoAddemail.message,
                    data = dtoAddemail.data,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to ISP: {ex.Message}");
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = ex.Message,
                    data = null,
                };
            }
        }

        public async Task<DtoResponse<EmailViewModel>> SendEmailUptime(
            NetworkViewModel net,
            Downtime dtm,
            IspViewModel isp,
            MessageViewModel msg
        )
        {
            try
            {
                string emailBody =
                    $@"
                    <html>
                    <body>
                        <p style=""font-size: 14px;"">Dear Rekan {isp.Name},</p>
                        <br>
                        <p style=""font-size: 14px;"">Terpantau link <strong>{net.Name}</strong> telah kembali <strong>Up</strong> pada <strong>{dtm.End}</strong>. Apakah proses perbaikan telah selesai?</p>
                        <br>
                    </body>
                    </html>";

                List<DetailEmailPicViewModel> detailEmailPics = [];
                List<DetailEmailHelpdeskViewModel> detailEmailHelpdesks = [];

                DtoResponse<List<PicViewModel>> dtoPicByRole = _picRepository.GetPicsByRole(2);

                if (!dtoPicByRole.status.Equals(200) || dtoPicByRole.data == null)
                {
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoPicByRole.message,
                        data = null,
                    };
                }

                List<PicViewModel> pics = dtoPicByRole.data;
                TimeSpan timeSpan = DateTime.Now - dtm.Start;

                foreach (PicViewModel pic in pics)
                {
                    detailEmailPics.Add(
                        new DetailEmailPicViewModel
                        {
                            PicId = pic.Id,
                            Type = timeSpan.TotalHours > 2 ? 1 : 2,
                            EmailAddress = pic.EmailAddress,
                            PicName = pic.Name,
                        }
                    );
                }

                if (timeSpan.TotalHours > 2)
                {
                    DtoResponse<List<HelpdeskViewModel>> dtoHelpdeskByIsp =
                        _helpdeskRepository.GetHelpdesksByIspId(isp.Id);

                    if (!dtoHelpdeskByIsp.status.Equals(200) || dtoHelpdeskByIsp.data == null)
                    {
                        return new DtoResponse<EmailViewModel>
                        {
                            status = 500,
                            message = dtoHelpdeskByIsp.message,
                            data = null,
                        };
                    }

                    List<HelpdeskViewModel> helpdesks = dtoHelpdeskByIsp.data;

                    foreach (HelpdeskViewModel helpdesk in helpdesks)
                    {
                        detailEmailHelpdesks.Add(
                            new DetailEmailHelpdeskViewModel
                            {
                                HelpdeskId = helpdesk.Id,
                                Type = 1,
                                EmailAddress = helpdesk.EmailAddress,
                                HelpdeskName = helpdesk.Name,
                            }
                        );
                    }
                }

                DtoResponse<string> dtoSendemail = await _smtpService.SendMailToIsp(
                    msg.Image,
                    $"{net.Name} Terpantau Down",
                    emailBody,
                    isp,
                    detailEmailPics,
                    detailEmailHelpdesks
                );

                if (!dtoSendemail.status.Equals(200) || dtoSendemail.data == null)
                {
                    var emailError = new Email
                    {
                        DowntimeId = dtm.Id,
                        Type = 1,
                        Subject = $"{net.Name} Terpantau Down",
                        Body = emailBody,
                        Image = msg.Image,
                        Date = DateTime.Now,
                        Status = 2,
                    };

                    List<DetailEmailPic> dtlEmailPicsError = [];
                    List<DetailEmailHelpdesk> dtlEmailHelpdesksError = [];
                    foreach (DetailEmailPicViewModel detailEmailPic in detailEmailPics)
                    {
                        dtlEmailPicsError.Add(
                            new DetailEmailPic
                            {
                                PicId = detailEmailPic.PicId,
                                Type = detailEmailPic.Type,
                                EmailAddress = detailEmailPic.EmailAddress,
                            }
                        );
                    }

                    foreach (
                        DetailEmailHelpdeskViewModel detailEmailHelpdesk in detailEmailHelpdesks
                    )
                    {
                        dtlEmailHelpdesksError.Add(
                            new DetailEmailHelpdesk
                            {
                                HelpdeskId = detailEmailHelpdesk.HelpdeskId,
                                Type = detailEmailHelpdesk.Type,
                                EmailAddress = detailEmailHelpdesk.EmailAddress,
                            }
                        );
                    }

                    DtoResponse<EmailViewModel> dtoAddemailError = _emailRepository.AddEmail(
                        emailError,
                        dtlEmailPicsError,
                        dtlEmailHelpdesksError,
                        dtm.CreatedBy
                    );

                    if (!dtoAddemailError.status.Equals(201) || dtoAddemailError.data == null)
                    {
                        return new DtoResponse<EmailViewModel>
                        {
                            status = 500,
                            message = dtoAddemailError.message,
                            data = null,
                        };
                    }

                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoSendemail.message,
                        data = null,
                    };
                }

                var email = new Email
                {
                    DowntimeId = dtm.Id,
                    Type = 2,
                    Subject = $"{net.Name} Terpantau Down",
                    Body = emailBody,
                    Image = msg.Image,
                    Date = DateTime.Now,
                    Status = 1,
                };

                List<DetailEmailPic> dtlEmailPics = [];
                List<DetailEmailHelpdesk> dtlEmailHelpdesks = [];
                foreach (DetailEmailPicViewModel detailEmailPic in detailEmailPics)
                {
                    dtlEmailPics.Add(
                        new DetailEmailPic
                        {
                            PicId = detailEmailPic.PicId,
                            Type = detailEmailPic.Type,
                            EmailAddress = detailEmailPic.EmailAddress,
                        }
                    );
                }

                foreach (DetailEmailHelpdeskViewModel detailEmailHelpdesk in detailEmailHelpdesks)
                {
                    dtlEmailHelpdesks.Add(
                        new DetailEmailHelpdesk
                        {
                            HelpdeskId = detailEmailHelpdesk.HelpdeskId,
                            Type = detailEmailHelpdesk.Type,
                            EmailAddress = detailEmailHelpdesk.EmailAddress,
                        }
                    );
                }

                DtoResponse<EmailViewModel> dtoAddemail = _emailRepository.AddEmail(
                    email,
                    dtlEmailPics,
                    dtlEmailHelpdesks,
                    dtm.UpdatedBy
                );

                if (!dtoAddemail.status.Equals(201) || dtoAddemail.data == null)
                {
                    return new DtoResponse<EmailViewModel>
                    {
                        status = 500,
                        message = dtoAddemail.message,
                        data = null,
                    };
                }

                return new DtoResponse<EmailViewModel>
                {
                    status = 200,
                    message = dtoAddemail.message,
                    data = dtoAddemail.data,
                };
            }
            catch (Exception emailEx)
            {
                return new DtoResponse<EmailViewModel>
                {
                    status = 500,
                    message = $"Failed to send email to ISP: {emailEx.Message}",
                    data = null,
                };
            }
        }
    }
}

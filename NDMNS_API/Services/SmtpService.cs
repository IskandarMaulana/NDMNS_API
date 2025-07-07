using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NDMNS_API.Models;
using NDMNS_API.Responses;

namespace NDMNS_API.Services
{
    public class SmtpService
    {
        private readonly HelperService _helperService;
        private readonly string _smtp;
        private readonly string _mailfrom;
        private readonly string _mailuser;
        private readonly string _mailauth;

        public SmtpService(IConfiguration config, HelperService helperService)
        {
            _helperService = helperService;

            _smtp = config["OutlookSmtp:SmtpMail"];
            _mailfrom = config["OutlookSmtp:MailFrom"];
            _mailuser = config["OutlookSmtp:MailUser"];
            _mailauth = config["OutlookSmtp:MailAuth"];
        }

        public async Task<DtoResponse<string>> SendMailToIsp(
            string base64Image,
            string subject,
            string htmlBody,
            IspViewModel recipientEmail,
            List<DetailEmailPicViewModel> detailEmailPics,
            List<DetailEmailHelpdeskViewModel> detailEmailHelpdesks
        )
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Network Operation Center", _mailfrom));
                message.To.Add(
                    new MailboxAddress(recipientEmail.Name, recipientEmail.EmailAddress)
                );

                foreach (DetailEmailPicViewModel detailEmailPic in detailEmailPics)
                {
                    if (detailEmailPic.Type.Equals(1))
                    {
                        message.To.Add(
                            new MailboxAddress(detailEmailPic.PicName, detailEmailPic.EmailAddress)
                        );
                    }
                    else if (detailEmailPic.Type.Equals(2))
                    {
                        message.Cc.Add(
                            new MailboxAddress(detailEmailPic.PicName, detailEmailPic.EmailAddress)
                        );
                    }
                }

                foreach (DetailEmailHelpdeskViewModel detailEmailHelpdesk in detailEmailHelpdesks)
                {
                    if (detailEmailHelpdesk.Type.Equals(1))
                    {
                        message.To.Add(
                            new MailboxAddress(
                                detailEmailHelpdesk.HelpdeskName,
                                detailEmailHelpdesk.EmailAddress
                            )
                        );
                    }
                    else if (detailEmailHelpdesk.Type.Equals(2))
                    {
                        message.Cc.Add(
                            new MailboxAddress(
                                detailEmailHelpdesk.HelpdeskName,
                                detailEmailHelpdesk.EmailAddress
                            )
                        );
                    }
                }

                message.Subject = subject;

                var now = DateTime.Now;

                var currentShiftName = await _helperService.GetCurrentShiftUserName(now);

                var builder = new BodyBuilder();

                string footerHtml =
                    $@"
                    <br><br>
                    <hr style=""border: 1px solid #ddd; margin: 20px 0;"">
                    <table style=""width: 100%; margin-top: 20px;"">
                        <tr>
                            <td style=""width: 140px;vertical-align: top; padding-right: 20px;"">
                                <img src=""cid:companyLogo"" alt=""AGIT Logo"" style=""width: 140px; height: auto;"" />
                            </td>
                            <td style=""padding-left: 5px; vertical-align: center; border-left: 2px solid black;"">
                                <p style=""margin: 0; font-size: 14px; color: #333;""><strong>Best Regards,</strong></p>
                                <br>
                                <p style=""margin: 0; font-size: 14px; color: #333;""><strong>{currentShiftName} - Network Operation Center</strong></p>
                                <p style=""margin: 0; font-size: 14px; color: #333;""><strong>Project Operation - Managed Service PT. PAMAPERSADA NUSANTARA</strong></p>
                                <p style=""margin: 0; font-size: 14px; color: #333;""><strong>Gedung PAMA 1 Lantai 3, JL. Rawagelam 1 No. 09, Jakarta Industrial Estate, Jakarta 13930</strong></p>
                                <p style=""margin: 0; font-size: 14px; color: #0000FF;"">
                                    Phone: +62 822-4959-4906 | 
                                    Email: <a href=""mailto:anum.fadhillah@ag-it.com"" style=""color: #0000FF; text-decoration: none;"">anum.fadhillah@ag-it.com</a> | 
                                    <a href=""http://www.ag-it.com"" style=""color: #0000FF; text-decoration: none;"">www.ag-it.com</a>
                                </p>
                            </td>
                        </tr>
                    </table>";

                string logoPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "agit-logo.png"
                );

                if (!string.IsNullOrEmpty(base64Image))
                {
                    try
                    {
                        byte[] imageBytes = Convert.FromBase64String(base64Image.Split(',')[1]);
                        string tempFilePath = Path.Combine(
                            Path.GetTempPath(),
                            $"network-alert-{Guid.NewGuid()}.png"
                        );
                        System.IO.File.WriteAllBytes(tempFilePath, imageBytes);

                        var imageAttachment = builder.LinkedResources.Add(tempFilePath);
                        imageAttachment.ContentId = "networkAlertImage";

                        if (File.Exists(logoPath))
                        {
                            var logoAttachment = builder.LinkedResources.Add(logoPath);
                            logoAttachment.ContentId = "companyLogo";
                        }

                        htmlBody = htmlBody.Replace(
                            "</body>",
                            @"<img src=""cid:networkAlertImage"" alt=""Network Status"" style=""max-width: 800px;"" />"
                                + footerHtml
                                + @"
                            </body>"
                        );

                        builder.HtmlBody = htmlBody;

                        using (var client = new SmtpClient())
                        {
                            client.Connect(_smtp, 587, SecureSocketOptions.StartTls);
                            client.Authenticate(_mailuser, _mailauth);
                            message.Body = builder.ToMessageBody();
                            client.Send(message);
                            client.Disconnect(true);
                        }

                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                    catch (Exception)
                    {
                        if (File.Exists(logoPath))
                        {
                            var logoAttachment = builder.LinkedResources.Add(logoPath);
                            logoAttachment.ContentId = "companyLogo";
                        }

                        htmlBody = htmlBody.Replace("</body>", footerHtml + "</body>");
                        builder.HtmlBody = htmlBody;

                        message.Body = builder.ToMessageBody();

                        using (var client = new SmtpClient())
                        {
                            client.Connect(_smtp, 587, SecureSocketOptions.StartTls);
                            client.Authenticate(_mailuser, _mailauth);
                            client.Send(message);
                            client.Disconnect(true);
                        }
                    }
                }
                else
                {
                    if (File.Exists(logoPath))
                    {
                        var logoAttachment = builder.LinkedResources.Add(logoPath);
                        logoAttachment.ContentId = "companyLogo";
                    }

                    htmlBody = htmlBody.Replace("</body>", footerHtml + "</body>");
                    builder.HtmlBody = htmlBody;

                    message.Body = builder.ToMessageBody();

                    using (var client = new SmtpClient())
                    {
                        client.Connect(_smtp, 587, SecureSocketOptions.StartTls);
                        client.Authenticate(_mailuser, _mailauth);
                        client.Send(message);
                        client.Disconnect(true);
                    }
                }

                return new DtoResponse<string>
                {
                    status = 200,
                    message = "Email sent to ISP successfully.",
                    data = subject,
                };
            }
            catch (Exception ex)
            {
                return new DtoResponse<string>
                {
                    status = 500,
                    message = $"Error sending email to ISP: {ex.Message}",
                    data = null,
                };
            }
        }
    }
}

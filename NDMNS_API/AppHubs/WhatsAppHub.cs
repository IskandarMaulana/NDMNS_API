using Microsoft.AspNetCore.SignalR;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.AppHubs
{
    public class WhatsAppHub : Hub
    {
        private readonly WhatsAppService _whatsAppService;

        public WhatsAppHub(WhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        public async Task UpdateQrCode(string qrCode)
        {
            await Clients.All.SendAsync(
                "ReceiveQrCode",
                new QrCodeResponse
                {
                    QrCode = qrCode,
                    IsReady = true,
                    Status = "QR Code ready for scanning",
                }
            );
        }

        public async Task ReceiveWhatsAppMessage(WhatsAppMessage messageData)
        {
            try
            {
                await _whatsAppService.DowntimeFollowUp(messageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task UpdateWhatsAppStatus(string whatsAppNumber, string status)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveStatus", status);
                _whatsAppService.UpdateStatus(whatsAppNumber, status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}

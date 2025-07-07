using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class WhatsAppController : ControllerBase
    {
        private readonly WhatsAppService _whatsAppService;

        public WhatsAppController(WhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        [HttpGet("status")]
        public async Task<ActionResult<DtoResponse<QrCodeResponse>>> GetQrCode()
        {
            try
            {
                var qrCodeResponse = await _whatsAppService.GetQrCodeAsync();
                return Ok(qrCodeResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Internal server error", error = ex.Message }
                );
            }
        }

        [HttpGet("groups")]
        public async Task<ActionResult<DtoResponse<List<GroupResponse>>>> GetGroups()
        {
            try
            {
                var groupsResponse = await _whatsAppService.GetGroupsAsync();
                return Ok(groupsResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { message = "Internal server error", error = ex.Message }
                );
            }
        }

        [HttpPost("alertDowntime")]
        public async Task<ActionResult<DtoResponse<WhatsAppMessage>>> SendMessageDowntime(
            [FromBody] SendRequest req
        )
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                if (string.IsNullOrWhiteSpace(userId) || userId == "System")
                {
                    var unauthorizedResponse = new DtoResponse<WhatsAppMessage>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }
                var result = await _whatsAppService.SendMessageDowntime(req, userId);

                return result.status switch
                {
                    200 => Ok(result),
                    201 => Created("", result),
                    204 => NoContent(),
                    400 => BadRequest(result),
                    401 => Unauthorized(),
                    403 => Forbid(),
                    404 => NotFound(result),
                    _ => StatusCode(result.status, result),
                };
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = $"Internal server error: {ex.Message}",
                    }
                );
            }
        }

        [HttpPost("alertUptime")]
        public async Task<ActionResult<DtoResponse<WhatsAppMessage>>> SendMessageUptime(
            [FromBody] SendRequest req
        )
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                var result = await _whatsAppService.SendMessageUptime(req, userId);

                return result.status switch
                {
                    200 => Ok(result),
                    201 => Created("", result),
                    204 => NoContent(),
                    400 => BadRequest(result),
                    401 => Unauthorized(),
                    403 => Forbid(),
                    404 => NotFound(result),
                    _ => StatusCode(result.status, result),
                };
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = $"Internal server error: {ex.Message}",
                    }
                );
            }
        }

        [HttpPost("alertUpdateDowntime")]
        public async Task<ActionResult<DtoResponse<WhatsAppMessage>>> SendMessageUpdateDowntime(
            [FromBody] SendRequest req
        )
        {
            try
            {
                var userId = Request.Headers["X-User-Id"].ToString();
                var result = await _whatsAppService.SendMessageUpdate(req, userId);

                return result.status switch
                {
                    200 => Ok(result),
                    201 => Created("", result),
                    204 => NoContent(),
                    400 => BadRequest(result),
                    401 => Unauthorized(),
                    403 => Forbid(),
                    404 => NotFound(result),
                    _ => StatusCode(result.status, result),
                };
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new DtoResponse<WhatsAppMessage>
                    {
                        status = 500,
                        message = $"Internal server error: {ex.Message}",
                    }
                );
            }
        }
    }
}

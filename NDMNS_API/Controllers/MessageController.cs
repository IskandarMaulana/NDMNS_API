using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly MessageRepository _messageRepository;

        public MessageController(MessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<MessageViewModel>>> GetAllMessages()
        {
            try
            {
                var result = _messageRepository.GetAllMessages();

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<List<MessageViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("downtime/{id}")]
        public ActionResult<DtoResponse<List<MessageViewModel>>> GetMessagesByDowntimeId(string id)
        {
            try
            {
                var result = _messageRepository.GetMessagesByDowntimeId(id);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<List<MessageViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("network/{id}")]
        public ActionResult<DtoResponse<List<MessageViewModel>>> GetMessagesByNetworkDown(string id)
        {
            try
            {
                var result = _messageRepository.GetMessagesByNetworkDown(id);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<List<MessageViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<MessageViewModel>> GetMessageById(string id)
        {
            try
            {
                var result = _messageRepository.GetMessageById(id);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("count")]
        public ActionResult<DtoResponse<int>> GetMessageCount()
        {
            try
            {
                var result = _messageRepository.GetSentMessageCount();

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<int>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = 0,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("messageid/{msgId}")]
        public ActionResult<DtoResponse<MessageViewModel>> GetMessageByMessageId(string msgId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(msgId))
                {
                    var badRequestResponse = new DtoResponse<MessageViewModel>
                    {
                        status = 400,
                        message = "Message ID cannot be empty",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var result = _messageRepository.GetMessageByMessageId(msgId);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<MessageViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }
    }
}

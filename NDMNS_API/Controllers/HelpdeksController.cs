using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelpdeskController : ControllerBase
    {
        private readonly HelpdeskRepository _helpdeskRepository;

        public HelpdeskController(HelpdeskRepository helpdeskRepository)
        {
            _helpdeskRepository = helpdeskRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<HelpdeskViewModel>>> GetAllHelpdesks()
        {
            try
            {
                var result = _helpdeskRepository.GetAllHelpdesks();

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
                var response = new DtoResponse<List<HelpdeskViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<HelpdeskViewModel>> GetHelpdeskById(string id)
        {
            try
            {
                var result = _helpdeskRepository.GetHelpdeskById(id);

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
                var response = new DtoResponse<HelpdeskViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("isp/{ispId}")]
        public ActionResult<DtoResponse<List<HelpdeskViewModel>>> GetHelpdesksByIspId(string ispId)
        {
            try
            {
                var result = _helpdeskRepository.GetHelpdesksByIspId(ispId);

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
                var response = new DtoResponse<List<HelpdeskViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<HelpdeskViewModel>> AddHelpdesk(
            [FromBody] HelpdeskViewModel helpdeskViewModel
        )
        {
            try
            {
                if (
                    helpdeskViewModel == null
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.Name)
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.IspId)
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.WhatsappNumber)
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.EmailAddress)
                )
                {
                    var badRequestResponse = new DtoResponse<HelpdeskViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid Helpdesk data. Name, IspId, WhatsappNumber, and EmailAddress are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                if (!_helpdeskRepository.IsIspExists(helpdeskViewModel.IspId))
                {
                    var badRequestResponse = new DtoResponse<HelpdeskViewModel>
                    {
                        status = 400,
                        message = "ISP not found",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<HelpdeskViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var helpdesk = new Helpdesk
                {
                    IspId = helpdeskViewModel.IspId,
                    Name = helpdeskViewModel.Name,
                    Role = helpdeskViewModel.Role,
                    WhatsappNumber = helpdeskViewModel.WhatsappNumber,
                    EmailAddress = helpdeskViewModel.EmailAddress,
                };

                var result = _helpdeskRepository.AddHelpdesk(helpdesk, createdBy);

                if (result.status == 201)
                {
                    return Created("", result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<HelpdeskViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<HelpdeskViewModel>> UpdateHelpdesk(
            string id,
            [FromBody] HelpdeskViewModel helpdeskViewModel
        )
        {
            try
            {
                if (
                    helpdeskViewModel == null
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.Name)
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.IspId)
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.WhatsappNumber)
                    || string.IsNullOrWhiteSpace(helpdeskViewModel.EmailAddress)
                )
                {
                    var badRequestResponse = new DtoResponse<HelpdeskViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid Helpdesk data. Name, IspId, WhatsappNumber, and EmailAddress are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var existingHelpdeskResult = _helpdeskRepository.GetHelpdeskById(id);
                if (existingHelpdeskResult.status == 404)
                {
                    return NotFound(existingHelpdeskResult);
                }
                else if (existingHelpdeskResult.status != 200)
                {
                    return StatusCode(existingHelpdeskResult.status, existingHelpdeskResult);
                }

                if (!_helpdeskRepository.IsIspExists(helpdeskViewModel.IspId))
                {
                    var badRequestResponse = new DtoResponse<HelpdeskViewModel>
                    {
                        status = 400,
                        message = "ISP not found",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<HelpdeskViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var helpdesk = new Helpdesk
                {
                    Id = id,
                    IspId = helpdeskViewModel.IspId,
                    Name = helpdeskViewModel.Name,
                    Role = helpdeskViewModel.Role,
                    WhatsappNumber = helpdeskViewModel.WhatsappNumber,
                    EmailAddress = helpdeskViewModel.EmailAddress,
                };

                var result = _helpdeskRepository.UpdateHelpdesk(helpdesk, updatedBy);

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
                var response = new DtoResponse<HelpdeskViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteHelpdesk(string id)
        {
            try
            {
                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<object>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var existingHelpdeskResult = _helpdeskRepository.GetHelpdeskById(id);
                if (existingHelpdeskResult.status == 404)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "Helpdesk not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }
                else if (existingHelpdeskResult.status != 200)
                {
                    var errorResponse = new DtoResponse<object>
                    {
                        status = existingHelpdeskResult.status,
                        message = existingHelpdeskResult.message,
                        data = null,
                    };
                    return StatusCode(existingHelpdeskResult.status, errorResponse);
                }

                var result = _helpdeskRepository.DeleteHelpdesk(id);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 409)
                {
                    return Conflict(result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<object>
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

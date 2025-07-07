using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IspController : ControllerBase
    {
        private readonly IspRepository _ispRepository;

        public IspController(IspRepository ispRepository)
        {
            _ispRepository = ispRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<IspViewModel>>> GetAllIsps()
        {
            try
            {
                var result = _ispRepository.GetAllIsps();

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
                var response = new DtoResponse<List<IspViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<IspViewModel>> GetIspById(string id)
        {
            try
            {
                var result = _ispRepository.GetIspById(id);

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
                var response = new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("network/{networkId}")]
        public ActionResult<DtoResponse<IspViewModel>> GetIspByNetworkId(string networkId)
        {
            try
            {
                var result = _ispRepository.GetIspByNetworkId(networkId);

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
                var response = new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<IspViewModel>> AddIsp([FromBody] IspViewModel ispViewModel)
        {
            try
            {
                if (ispViewModel == null || string.IsNullOrWhiteSpace(ispViewModel.Name))
                {
                    var badRequestResponse = new DtoResponse<IspViewModel>
                    {
                        status = 400,
                        message = "Invalid ISP data",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<IspViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var isp = new Isp
                {
                    Name = ispViewModel.Name,
                    WhatsappGroup = ispViewModel.WhatsappGroup,
                    EmailAddress = ispViewModel.EmailAddress,
                };

                var result = _ispRepository.AddIsp(isp, createdBy);

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
                var response = new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<IspViewModel>> UpdateIsp(
            string id,
            [FromBody] IspViewModel ispViewModel
        )
        {
            try
            {
                if (ispViewModel == null || string.IsNullOrWhiteSpace(ispViewModel.Name))
                {
                    var badRequestResponse = new DtoResponse<IspViewModel>
                    {
                        status = 400,
                        message = "Invalid ISP data",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<IspViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var existingIspResult = _ispRepository.GetIspById(id);
                if (existingIspResult.status == 404)
                {
                    return NotFound(existingIspResult);
                }
                else if (existingIspResult.status != 200)
                {
                    return StatusCode(existingIspResult.status, existingIspResult);
                }

                var isp = new Isp
                {
                    Id = id,
                    Name = ispViewModel.Name,
                    WhatsappGroup = ispViewModel.WhatsappGroup,
                    EmailAddress = ispViewModel.EmailAddress,
                };

                var result = _ispRepository.UpdateIsp(isp, updatedBy);

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
                var response = new DtoResponse<IspViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteIsp(string id)
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

                var existingIspResult = _ispRepository.GetIspById(id);
                if (existingIspResult.status == 404)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "ISP not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }
                else if (existingIspResult.status != 200)
                {
                    var errorResponse = new DtoResponse<object>
                    {
                        status = existingIspResult.status,
                        message = existingIspResult.message,
                        data = null,
                    };
                    return StatusCode(existingIspResult.status, errorResponse);
                }

                var result = _ispRepository.DeleteIsp(id);

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

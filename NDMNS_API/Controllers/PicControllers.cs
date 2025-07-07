using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PicController : ControllerBase
    {
        private readonly PicRepository _picRepository;

        public PicController(PicRepository picRepository)
        {
            _picRepository = picRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<PicViewModel>>> GetAllPics()
        {
            try
            {
                var result = _picRepository.GetAllPics();

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
                var response = new DtoResponse<List<PicViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<PicViewModel>> GetPicById(string id)
        {
            try
            {
                var result = _picRepository.GetPicById(id);

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
                var response = new DtoResponse<PicViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("site/{siteId}")]
        public ActionResult<DtoResponse<List<PicViewModel>>> GetPicsBySiteId(string siteId)
        {
            try
            {
                var result = _picRepository.GetPicsBySiteId(siteId);

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
                var response = new DtoResponse<List<PicViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<PicViewModel>> AddPic([FromBody] PicViewModel picViewModel)
        {
            try
            {
                if (
                    picViewModel == null
                    || string.IsNullOrWhiteSpace(picViewModel.Nrp)
                    || string.IsNullOrWhiteSpace(picViewModel.Name)
                    || string.IsNullOrWhiteSpace(picViewModel.SiteId)
                    || string.IsNullOrWhiteSpace(picViewModel.WhatsappNumber)
                )
                {
                    var badRequestResponse = new DtoResponse<PicViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid PIC data. Nrp, Name, Role, Site, and Whatsapp Number are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                if (!_picRepository.IsSiteExists(picViewModel.SiteId))
                {
                    var badRequestResponse = new DtoResponse<PicViewModel>
                    {
                        status = 400,
                        message = "Site not found",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<PicViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var pic = new Pic
                {
                    SiteId = picViewModel.SiteId,
                    Nrp = picViewModel.Nrp,
                    Name = picViewModel.Name,
                    Role = picViewModel.Role,
                    WhatsappNumber = picViewModel.WhatsappNumber,
                    EmailAddress = picViewModel.EmailAddress,
                };

                var result = _picRepository.AddPic(pic, createdBy);

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
                var response = new DtoResponse<PicViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<PicViewModel>> UpdatePic(
            string id,
            [FromBody] PicViewModel picViewModel
        )
        {
            try
            {
                if (
                    picViewModel == null
                    || string.IsNullOrWhiteSpace(picViewModel.Name)
                    || string.IsNullOrWhiteSpace(picViewModel.SiteId)
                    || string.IsNullOrWhiteSpace(picViewModel.WhatsappNumber)
                    || string.IsNullOrWhiteSpace(picViewModel.EmailAddress)
                )
                {
                    var badRequestResponse = new DtoResponse<PicViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid PIC data. Name, SiteId, WhatsappNumber, and EmailAddress are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var existingPicResult = _picRepository.GetPicById(id);
                if (existingPicResult.status == 404)
                {
                    return NotFound(existingPicResult);
                }
                else if (existingPicResult.status != 200)
                {
                    return StatusCode(existingPicResult.status, existingPicResult);
                }

                if (!_picRepository.IsSiteExists(picViewModel.SiteId))
                {
                    var badRequestResponse = new DtoResponse<PicViewModel>
                    {
                        status = 400,
                        message = "Site not found",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<PicViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var pic = new Pic
                {
                    Id = id,
                    SiteId = picViewModel.SiteId,
                    Nrp = picViewModel.Nrp,
                    Name = picViewModel.Name,
                    Role = picViewModel.Role,
                    WhatsappNumber = picViewModel.WhatsappNumber,
                    EmailAddress = picViewModel.EmailAddress,
                };

                var result = _picRepository.UpdatePic(pic, updatedBy);

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
                var response = new DtoResponse<PicViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeletePic(string id)
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

                var existingPicResult = _picRepository.GetPicById(id);
                if (existingPicResult.status == 404)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "PIC not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }
                else if (existingPicResult.status != 200)
                {
                    var errorResponse = new DtoResponse<object>
                    {
                        status = existingPicResult.status,
                        message = existingPicResult.message,
                        data = null,
                    };
                    return StatusCode(existingPicResult.status, errorResponse);
                }

                var result = _picRepository.DeletePic(id);

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

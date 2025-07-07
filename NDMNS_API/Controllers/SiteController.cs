using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly SiteRepository _siteRepository;
        private readonly WhatsAppService _whatsAppService;

        public SiteController(SiteRepository siteRepository, WhatsAppService whatsAppService)
        {
            _siteRepository = siteRepository;
            _whatsAppService = whatsAppService;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<SiteViewModel>>> GetAllSites()
        {
            try
            {
                Console.WriteLine("Requested GET");

                var result = _siteRepository.GetAllSites();

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
                var response = new DtoResponse<List<SiteViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<SiteViewModel>> GetSiteById(string id)
        {
            try
            {
                var result = _siteRepository.GetSiteById(id);

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
                var response = new DtoResponse<SiteViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<SiteViewModel>> AddSite(
            [FromBody] SiteViewModel siteViewModel
        )
        {
            try
            {
                if (siteViewModel == null || string.IsNullOrWhiteSpace(siteViewModel.Name))
                {
                    Console.WriteLine("Requested but null");

                    var badRequestResponse = new DtoResponse<SiteViewModel>
                    {
                        status = 400,
                        message = "Invalid site data",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<SiteViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var site = new Site
                {
                    Name = siteViewModel.Name,
                    WhatsappGroup = siteViewModel.WhatsappGroup,
                    Location = siteViewModel.Location,
                };

                var result = _siteRepository.AddSite(site, createdBy);

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
                var response = new DtoResponse<SiteViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<SiteViewModel>> UpdateSite(
            string id,
            [FromBody] SiteViewModel siteViewModel
        )
        {
            try
            {
                Console.WriteLine("Requested PUT");

                if (siteViewModel == null || string.IsNullOrWhiteSpace(siteViewModel.Name))
                {
                    var badRequestResponse = new DtoResponse<SiteViewModel>
                    {
                        status = 400,
                        message = "Invalid site data",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<SiteViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var existingSiteResult = _siteRepository.GetSiteById(id);
                if (existingSiteResult.status == 404)
                {
                    return NotFound(existingSiteResult);
                }
                else if (existingSiteResult.status != 200)
                {
                    return StatusCode(existingSiteResult.status, existingSiteResult);
                }

                var site = new Site
                {
                    Id = id,
                    Name = siteViewModel.Name,
                    WhatsappGroup = siteViewModel.WhatsappGroup,
                    Location = siteViewModel.Location,
                };

                var result = _siteRepository.UpdateSite(site, updatedBy);

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
                var response = new DtoResponse<SiteViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteSite(string id)
        {
            try
            {
                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<SiteViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var existingSiteResult = _siteRepository.GetSiteById(id);
                if (existingSiteResult.status == 404)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "Site not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }
                else if (existingSiteResult.status != 200)
                {
                    var errorResponse = new DtoResponse<object>
                    {
                        status = existingSiteResult.status,
                        message = existingSiteResult.message,
                        data = null,
                    };
                    return StatusCode(existingSiteResult.status, errorResponse);
                }

                var result = _siteRepository.DeleteSite(id);

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

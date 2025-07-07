using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkController : ControllerBase
    {
        private readonly NetworkRepository _networkRepository;

        public NetworkController(NetworkRepository networkRepository)
        {
            _networkRepository = networkRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<NetworkViewModel>>> GetAllNetworks()
        {
            try
            {
                var result = _networkRepository.GetAllNetworks();

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
                var response = new DtoResponse<List<NetworkViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<NetworkViewModel>> GetNetworkById(string id)
        {
            try
            {
                var result = _networkRepository.GetNetworkById(id);

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
                var response = new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("site/{siteId}")]
        public ActionResult<DtoResponse<List<NetworkViewModel>>> GetNetworksBySite(string siteId)
        {
            try
            {
                var result = _networkRepository.GetNetworksBySite(siteId);

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
                var response = new DtoResponse<List<NetworkViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("isp/{ispId}")]
        public ActionResult<DtoResponse<List<NetworkViewModel>>> GetNetworksByIsp(string ispId)
        {
            try
            {
                var result = _networkRepository.GetNetworksByIsp(ispId);

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
                var response = new DtoResponse<List<NetworkViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<NetworkViewModel>> AddNetwork(
            [FromBody] NetworkViewModel networkViewModel
        )
        {
            try
            {
                if (
                    networkViewModel == null
                    || string.IsNullOrWhiteSpace(networkViewModel.Name)
                    || string.IsNullOrWhiteSpace(networkViewModel.Ip)
                    || string.IsNullOrWhiteSpace(networkViewModel.SiteId)
                    || string.IsNullOrWhiteSpace(networkViewModel.IspId)
                    || string.IsNullOrWhiteSpace(networkViewModel.Cid)
                )
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid Network data. Name, IP, SiteId, IspId, and Cid are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                if (!_networkRepository.IsSiteExists(networkViewModel.SiteId))
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 400,
                        message = "Site not found",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                if (!_networkRepository.IsIspExists(networkViewModel.IspId))
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
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
                    var unauthorizedResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var network = new Network
                {
                    Name = networkViewModel.Name,
                    Ip = networkViewModel.Ip,
                    Status = networkViewModel.Status,
                    SiteId = networkViewModel.SiteId,
                    IspId = networkViewModel.IspId,
                    Cid = networkViewModel.Cid,
                    Latency = networkViewModel.Latency,
                    LastUpdate = DateTime.Now,
                };

                var result = _networkRepository.AddNetwork(network, createdBy);

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
                var response = new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<NetworkViewModel>> UpdateNetwork(
            string id,
            [FromBody] NetworkViewModel networkViewModel
        )
        {
            try
            {
                if (
                    networkViewModel == null
                    || string.IsNullOrWhiteSpace(networkViewModel.Name)
                    || string.IsNullOrWhiteSpace(networkViewModel.Ip)
                    || string.IsNullOrWhiteSpace(networkViewModel.SiteId)
                    || string.IsNullOrWhiteSpace(networkViewModel.IspId)
                    || string.IsNullOrWhiteSpace(networkViewModel.Cid)
                )
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid Network data. Name, IP, SiteId, IspId, and Cid are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var existingNetworkResult = _networkRepository.GetNetworkById(id);
                if (existingNetworkResult.status == 404)
                {
                    return NotFound(existingNetworkResult);
                }
                else if (existingNetworkResult.status != 200)
                {
                    return StatusCode(existingNetworkResult.status, existingNetworkResult);
                }

                if (!_networkRepository.IsSiteExists(networkViewModel.SiteId))
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 400,
                        message = "Site not found",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                if (!_networkRepository.IsIspExists(networkViewModel.IspId))
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
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
                    var unauthorizedResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var network = new Network
                {
                    Id = id,
                    Name = networkViewModel.Name,
                    Ip = networkViewModel.Ip,
                    Status = networkViewModel.Status,
                    SiteId = networkViewModel.SiteId,
                    IspId = networkViewModel.IspId,
                    Cid = networkViewModel.Cid,
                };

                var result = _networkRepository.UpdateNetwork(network, updatedBy);

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
                var response = new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}/ping")]
        public ActionResult<DtoResponse<NetworkViewModel>> UpdateNetworkPing(
            string id,
            [FromBody] NetworkViewModel networkViewModel
        )
        {
            try
            {
                if (networkViewModel == null)
                {
                    var badRequestResponse = new DtoResponse<NetworkViewModel>
                    {
                        status = 400,
                        message = "Invalid network ping data",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var existingNetworkResult = _networkRepository.GetNetworkById(id);
                if (existingNetworkResult.status == 404)
                {
                    return NotFound(existingNetworkResult);
                }
                else if (existingNetworkResult.status != 200)
                {
                    return StatusCode(existingNetworkResult.status, existingNetworkResult);
                }

                var network = new Network
                {
                    Id = id,
                    Status = networkViewModel.Status,
                    Latency = networkViewModel.Latency,
                    LastUpdate = DateTime.Now,
                };

                var result = _networkRepository.UpdateNetworkPing(network);

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
                var response = new DtoResponse<NetworkViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteNetwork(string id)
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

                var existingNetworkResult = _networkRepository.GetNetworkById(id);
                if (existingNetworkResult.status == 404)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "Network not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }
                else if (existingNetworkResult.status != 200)
                {
                    var errorResponse = new DtoResponse<object>
                    {
                        status = existingNetworkResult.status,
                        message = existingNetworkResult.message,
                        data = null,
                    };
                    return StatusCode(existingNetworkResult.status, errorResponse);
                }

                var result = _networkRepository.DeleteNetwork(id);

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

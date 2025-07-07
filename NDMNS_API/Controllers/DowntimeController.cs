using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DowntimeController : ControllerBase
    {
        private readonly DowntimeRepository _downtimeRepository;

        public DowntimeController(DowntimeRepository downtimeRepository)
        {
            _downtimeRepository = downtimeRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<DowntimeViewModel>>> GetAllDowntimes()
        {
            try
            {
                var result = _downtimeRepository.GetAllDowntimes();

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
                var response = new DtoResponse<List<DowntimeViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<DowntimeViewModel>> GetDowntimeById(string id)
        {
            try
            {
                var result = _downtimeRepository.GetDowntimeById(id);

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
                var response = new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("network/{networkId}")]
        public ActionResult<DtoResponse<List<DowntimeViewModel>>> GetDowntimesByNetworkId(
            string networkId
        )
        {
            try
            {
                var result = _downtimeRepository.GetDowntimesByNetworkId(networkId);

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
                var response = new DtoResponse<List<DowntimeViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("network/{networkId}/latest")]
        public ActionResult<DtoResponse<DowntimeViewModel>> GetLatestDowntimeByNetworkId(
            string networkId
        )
        {
            try
            {
                var result = _downtimeRepository.GetLatestDowntimeByNetworkId(networkId);

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
                var response = new DtoResponse<DowntimeViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        // [HttpPost]
        // public ActionResult<DtoResponse<DowntimeViewModel>> AddDowntime(
        //     [FromBody] DowntimeViewModel downtimeViewModel
        // )
        // {
        //     try
        //     {
        //         if (
        //             downtimeViewModel == null
        //             || string.IsNullOrWhiteSpace(downtimeViewModel.Description)
        //             || string.IsNullOrWhiteSpace(downtimeViewModel.NetworkId)
        //             || string.IsNullOrWhiteSpace(downtimeViewModel.TicketNumber)
        //         )
        //         {
        //             var badRequestResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 400,
        //                 message =
        //                     "Invalid downtime data. Description, NetworkId, and TicketNumber are required.",
        //                 data = null,
        //             };
        //             return BadRequest(badRequestResponse);
        //         }

        //         if (!_downtimeRepository.IsNetworkExists(downtimeViewModel.NetworkId))
        //         {
        //             var badRequestResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 400,
        //                 message = "Network not found",
        //                 data = null,
        //             };
        //             return BadRequest(badRequestResponse);
        //         }

        //         var createdBy = Request.Headers["X-User-Id"].ToString();

        //         if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
        //         {
        //             var unauthorizedResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 401,
        //                 message = "User not found. Please login first.",
        //                 data = null,
        //             };
        //             return Unauthorized(unauthorizedResponse);
        //         }

        //         var downtime = new Downtime
        //         {
        //             NetworkId = downtimeViewModel.NetworkId,
        //             Description = downtimeViewModel.Description,
        //             TicketNumber = downtimeViewModel.TicketNumber,
        //             Date = downtimeViewModel.Date,
        //             Start = downtimeViewModel.Start,
        //             End = downtimeViewModel.End,
        //             Category = downtimeViewModel.Category,
        //             Subcategory = downtimeViewModel.Subcategory,
        //             Action = downtimeViewModel.Action,
        //             Status = downtimeViewModel.Status,
        //         };

        //         var result = _downtimeRepository.AddDowntime(downtime, createdBy);

        //         if (result.status == 201)
        //         {
        //             return Created("", result);
        //         }
        //         else
        //         {
        //             return StatusCode(result.status, result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         var response = new DtoResponse<DowntimeViewModel>
        //         {
        //             status = 500,
        //             message = $"Internal server error: {ex.Message}",
        //             data = null,
        //         };
        //         return StatusCode(500, response);
        //     }
        // }

        // [HttpPut("{id}")]
        // public ActionResult<DtoResponse<DowntimeViewModel>> UpdateDowntime(
        //     string id,
        //     [FromBody] DowntimeViewModel downtimeViewModel
        // )
        // {
        //     try
        //     {
        //         if (
        //             downtimeViewModel == null
        //             || string.IsNullOrWhiteSpace(downtimeViewModel.Description)
        //             || string.IsNullOrWhiteSpace(downtimeViewModel.NetworkId)
        //             || string.IsNullOrWhiteSpace(downtimeViewModel.TicketNumber)
        //         )
        //         {
        //             var badRequestResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 400,
        //                 message =
        //                     "Invalid downtime data. Description, NetworkId, and TicketNumber are required.",
        //                 data = null,
        //             };
        //             return BadRequest(badRequestResponse);
        //         }

        //         var existingDowntimeResult = _downtimeRepository.GetDowntimeById(id);
        //         if (existingDowntimeResult.status == 404)
        //         {
        //             return NotFound(existingDowntimeResult);
        //         }
        //         else if (existingDowntimeResult.status != 200)
        //         {
        //             return StatusCode(existingDowntimeResult.status, existingDowntimeResult);
        //         }

        //         if (!_downtimeRepository.IsNetworkExists(downtimeViewModel.NetworkId))
        //         {
        //             var badRequestResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 400,
        //                 message = "Network not found",
        //                 data = null,
        //             };
        //             return BadRequest(badRequestResponse);
        //         }

        //         var updatedBy = Request.Headers["X-User-Id"].ToString();

        //         if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
        //         {
        //             var unauthorizedResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 401,
        //                 message = "User session not found. Please login first.",
        //                 data = null,
        //             };
        //             return Unauthorized(unauthorizedResponse);
        //         }

        //         var downtime = new Downtime
        //         {
        //             Id = id,
        //             NetworkId = downtimeViewModel.NetworkId,
        //             Description = downtimeViewModel.Description,
        //             TicketNumber = downtimeViewModel.TicketNumber,
        //             Date = downtimeViewModel.Date,
        //             Start = downtimeViewModel.Start,
        //             End = downtimeViewModel.End,
        //             Category = downtimeViewModel.Category,
        //             Subcategory = downtimeViewModel.Subcategory,
        //             Action = downtimeViewModel.Action,
        //             Status = downtimeViewModel.Status,
        //         };

        //         var result = _downtimeRepository.UpdateDowntime(downtime, updatedBy);

        //         if (result.status == 200)
        //         {
        //             return Ok(result);
        //         }
        //         else if (result.status == 404)
        //         {
        //             return NotFound(result);
        //         }
        //         else
        //         {
        //             return StatusCode(result.status, result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         var response = new DtoResponse<DowntimeViewModel>
        //         {
        //             status = 500,
        //             message = $"Internal server error: {ex.Message}",
        //             data = null,
        //         };
        //         return StatusCode(500, response);
        //     }
        // }

        // [HttpPatch("{id}/category")]
        // public ActionResult<DtoResponse<DowntimeViewModel>> UpdateDowntimeCategory(
        //     string id,
        //     [FromBody] DowntimeViewModel downtimeViewModel
        // )
        // {
        //     try
        //     {
        //         if (downtimeViewModel == null)
        //         {
        //             var badRequestResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 400,
        //                 message = "Invalid downtime data",
        //                 data = null,
        //             };
        //             return BadRequest(badRequestResponse);
        //         }

        //         var existingDowntimeResult = _downtimeRepository.GetDowntimeById(id);
        //         if (existingDowntimeResult.status == 404)
        //         {
        //             return NotFound(existingDowntimeResult);
        //         }
        //         else if (existingDowntimeResult.status != 200)
        //         {
        //             return StatusCode(existingDowntimeResult.status, existingDowntimeResult);
        //         }

        //         var updatedBy = Request.Headers["X-User-Id"].ToString();

        //         if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
        //         {
        //             var unauthorizedResponse = new DtoResponse<DowntimeViewModel>
        //             {
        //                 status = 401,
        //                 message = "User session not found. Please login first.",
        //                 data = null,
        //             };
        //             return Unauthorized(unauthorizedResponse);
        //         }

        //         var downtime = new Downtime
        //         {
        //             Id = id,
        //             Category = downtimeViewModel.Category,
        //             Subcategory = downtimeViewModel.Subcategory,
        //         };

        //         var result = _downtimeRepository.UpdateDowntimeCategory(downtime, updatedBy);

        //         if (result.status == 200)
        //         {
        //             return Ok(result);
        //         }
        //         else if (result.status == 404)
        //         {
        //             return NotFound(result);
        //         }
        //         else
        //         {
        //             return StatusCode(result.status, result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         var response = new DtoResponse<DowntimeViewModel>
        //         {
        //             status = 500,
        //             message = $"Internal server error: {ex.Message}",
        //             data = null,
        //         };
        //         return StatusCode(500, response);
        //     }
        // }

        // [HttpDelete("{id}")]
        // public ActionResult<DtoResponse<object>> DeleteDowntime(string id)
        // {
        //     try
        //     {
        //         var updatedBy = Request.Headers["X-User-Id"].ToString();

        //         if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
        //         {
        //             var unauthorizedResponse = new DtoResponse<object>
        //             {
        //                 status = 401,
        //                 message = "User session not found. Please login first.",
        //                 data = null,
        //             };
        //             return Unauthorized(unauthorizedResponse);
        //         }

        //         var existingDowntimeResult = _downtimeRepository.GetDowntimeById(id);
        //         if (existingDowntimeResult.status == 404)
        //         {
        //             var notFoundResponse = new DtoResponse<object>
        //             {
        //                 status = 404,
        //                 message = "Downtime not found",
        //                 data = null,
        //             };
        //             return NotFound(notFoundResponse);
        //         }
        //         else if (existingDowntimeResult.status != 200)
        //         {
        //             var errorResponse = new DtoResponse<object>
        //             {
        //                 status = existingDowntimeResult.status,
        //                 message = existingDowntimeResult.message,
        //                 data = null,
        //             };
        //             return StatusCode(existingDowntimeResult.status, errorResponse);
        //         }

        //         var result = _downtimeRepository.DeleteDowntime(id);

        //         if (result.status == 200)
        //         {
        //             return Ok(result);
        //         }
        //         else if (result.status == 409)
        //         {
        //             return Conflict(result);
        //         }
        //         else
        //         {
        //             return StatusCode(result.status, result);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         var response = new DtoResponse<object>
        //         {
        //             status = 500,
        //             message = $"Internal server error: {ex.Message}",
        //             data = null,
        //         };
        //         return StatusCode(500, response);
        //     }
        // }
    }
}

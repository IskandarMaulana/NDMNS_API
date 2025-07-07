using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly ShiftRepository _shiftRepository;

        public ShiftController(ShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<ShiftViewModel>>> GetAllShifts()
        {
            try
            {
                var result = _shiftRepository.GetAllShifts();

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
                var response = new DtoResponse<List<ShiftViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<ShiftViewModel>> GetShiftById(string id)
        {
            try
            {
                var result = _shiftRepository.GetShiftById(id);

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
                var response = new DtoResponse<ShiftViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("user/{userId}")]
        public ActionResult<DtoResponse<List<ShiftViewModel>>> GetShiftsByUserId(string userId)
        {
            try
            {
                var result = _shiftRepository.GetShiftsByUserId(userId);

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
                var response = new DtoResponse<List<ShiftViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("daterange")]
        public ActionResult<DtoResponse<List<ShiftViewModel>>> GetShiftsByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate
        )
        {
            try
            {
                if (startDate == default(DateTime) || endDate == default(DateTime))
                {
                    var badRequestResponse = new DtoResponse<List<ShiftViewModel>>
                    {
                        status = 400,
                        message = "Start date and end date are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                if (startDate > endDate)
                {
                    var badRequestResponse = new DtoResponse<List<ShiftViewModel>>
                    {
                        status = 400,
                        message = "Start date cannot be greater than end date.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var result = _shiftRepository.GetShiftsByDateRange(startDate, endDate);

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
                var response = new DtoResponse<List<ShiftViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<ShiftViewModel>> AddShift(
            [FromBody] ShiftViewModel shiftViewModel
        )
        {
            try
            {
                if (
                    shiftViewModel == null
                    || string.IsNullOrWhiteSpace(shiftViewModel.UserId)
                    || shiftViewModel.StartDate == default(DateTime)
                    || shiftViewModel.EndDate == default(DateTime)
                    || shiftViewModel.StartTime == default(TimeSpan)
                    || shiftViewModel.EndTime == default(TimeSpan)
                )
                {
                    var badRequestResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid Shift data. UserId, StartDate, EndDate, StartTime, and EndTime are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                // Validasi tanggal mulai tidak boleh lebih dari tanggal selesai
                if (shiftViewModel.StartDate > shiftViewModel.EndDate)
                {
                    var badRequestResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Start date cannot be greater than end date.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                // Validasi waktu mulai tidak boleh sama dengan waktu selesai
                if (shiftViewModel.StartTime == shiftViewModel.EndTime)
                {
                    var badRequestResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Start time cannot be the same as end time.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var shift = new Shift
                {
                    UserId = shiftViewModel.UserId,
                    StartDate = shiftViewModel.StartDate,
                    EndDate = shiftViewModel.EndDate,
                    StartTime = shiftViewModel.StartTime,
                    EndTime = shiftViewModel.EndTime,
                };

                var result = _shiftRepository.AddShift(shift, createdBy);

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
                var response = new DtoResponse<ShiftViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<ShiftViewModel>> UpdateShift(
            string id,
            [FromBody] ShiftViewModel shiftViewModel
        )
        {
            try
            {
                if (
                    shiftViewModel == null
                    || string.IsNullOrWhiteSpace(shiftViewModel.UserId)
                    || shiftViewModel.StartDate == default(DateTime)
                    || shiftViewModel.EndDate == default(DateTime)
                    || shiftViewModel.StartTime == default(TimeSpan)
                    || shiftViewModel.EndTime == default(TimeSpan)
                )
                {
                    var badRequestResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid Shift data. UserId, StartDate, EndDate, StartTime, and EndTime are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                // Validasi tanggal mulai tidak boleh lebih dari tanggal selesai
                if (shiftViewModel.StartDate > shiftViewModel.EndDate)
                {
                    var badRequestResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Start date cannot be greater than end date.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                // Validasi waktu mulai tidak boleh sama dengan waktu selesai
                if (shiftViewModel.StartTime == shiftViewModel.EndTime)
                {
                    var badRequestResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 400,
                        message = "Start time cannot be the same as end time.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var existingShiftResult = _shiftRepository.GetShiftById(id);
                if (existingShiftResult.status == 404)
                {
                    return NotFound(existingShiftResult);
                }
                else if (existingShiftResult.status != 200)
                {
                    return StatusCode(existingShiftResult.status, existingShiftResult);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<ShiftViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var shift = new Shift
                {
                    Id = id,
                    UserId = shiftViewModel.UserId,
                    StartDate = shiftViewModel.StartDate,
                    EndDate = shiftViewModel.EndDate,
                    StartTime = shiftViewModel.StartTime,
                    EndTime = shiftViewModel.EndTime,
                };

                var result = _shiftRepository.UpdateShift(shift, updatedBy);

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
                var response = new DtoResponse<ShiftViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteShift(string id)
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

                var existingShiftResult = _shiftRepository.GetShiftById(id);
                if (existingShiftResult.status == 404)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "Shift not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }
                else if (existingShiftResult.status != 200)
                {
                    var errorResponse = new DtoResponse<object>
                    {
                        status = existingShiftResult.status,
                        message = existingShiftResult.message,
                        data = null,
                    };
                    return StatusCode(existingShiftResult.status, errorResponse);
                }

                var result = _shiftRepository.DeleteShift(id);

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

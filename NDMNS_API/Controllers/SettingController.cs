using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;
using NDMNS_API.Services;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : ControllerBase
    {
        private readonly SettingRepository _settingRepository;
        private readonly SystemHealthCheckService _systemHealthCheckService;

        public SettingController(
            SettingRepository settingRepository,
            SystemHealthCheckService systemHealthCheckService
        )
        {
            _settingRepository = settingRepository;
            _systemHealthCheckService = systemHealthCheckService;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<SettingViewModel>>> GetAllSettings()
        {
            try
            {
                var settings = _settingRepository.GetAllSettings();
                var response = new DtoResponse<List<SettingViewModel>>
                {
                    status = 200,
                    message = "Settings retrieved successfully",
                    data = settings,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<List<SettingViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<SettingViewModel>> GetSettingById(string id)
        {
            try
            {
                var setting = _settingRepository.GetSettingById(id);
                if (setting == null)
                {
                    var notFoundResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 404,
                        message = "Setting not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }

                var response = new DtoResponse<SettingViewModel>
                {
                    status = 200,
                    message = "Setting retrieved successfully",
                    data = setting,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<SettingViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("code/{code}")]
        public ActionResult<DtoResponse<SettingViewModel>> GetSettingByCode(string code)
        {
            try
            {
                var setting = _settingRepository.GetSettingByCode(code);
                if (setting == null)
                {
                    var notFoundResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 404,
                        message = "Setting not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }

                var response = new DtoResponse<SettingViewModel>
                {
                    status = 200,
                    message = "Setting retrieved successfully",
                    data = setting,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<SettingViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("health")]
        public async Task<ActionResult<DtoResponse<SystemHealthResult>>> GetSystemHealth()
        {
            try
            {
                var healthStatus = await _systemHealthCheckService.GetSystemHealthStatusAsync();

                var response = new DtoResponse<SystemHealthResult>
                {
                    status = 200,
                    message = "System health retrieved successfully",
                    data = healthStatus,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<SystemHealthResult>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<SettingViewModel>> AddSetting(
            [FromBody] SettingViewModel settingViewModel
        )
        {
            try
            {
                if (
                    settingViewModel == null
                    || string.IsNullOrWhiteSpace(settingViewModel.Name)
                    || string.IsNullOrWhiteSpace(settingViewModel.Code)
                    || string.IsNullOrWhiteSpace(settingViewModel.Value)
                )
                {
                    var badRequestResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 400,
                        message = "Invalid setting data. Name, Code, and Value are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                // Check if code already exists
                if (_settingRepository.IsCodeExists(settingViewModel.Code))
                {
                    var conflictResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 409,
                        message = "Setting code already exists",
                        data = null,
                    };
                    return Conflict(conflictResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var setting = new Setting
                {
                    Name = settingViewModel.Name,
                    Code = settingViewModel.Code,
                    Value = settingViewModel.Value,
                };

                _settingRepository.AddSetting(setting, createdBy);

                var response = new DtoResponse<SettingViewModel>
                {
                    status = 201,
                    message = "Setting data saved successfully",
                    data = settingViewModel,
                };
                return Created("", response);
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<SettingViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<SettingViewModel>> UpdateSetting(
            string id,
            [FromBody] SettingViewModel settingViewModel
        )
        {
            try
            {
                if (
                    settingViewModel == null
                    || string.IsNullOrWhiteSpace(settingViewModel.Name)
                    || string.IsNullOrWhiteSpace(settingViewModel.Code)
                    || string.IsNullOrWhiteSpace(settingViewModel.Value)
                )
                {
                    var badRequestResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 400,
                        message = "Invalid setting data. Name, Code, and Value are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var existingSetting = _settingRepository.GetSettingById(id);
                if (existingSetting == null)
                {
                    var notFoundResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 404,
                        message = "Setting not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }

                // Check if code already exists (excluding current setting)
                if (_settingRepository.IsCodeExists(settingViewModel.Code, id))
                {
                    var conflictResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 409,
                        message = "Setting code already exists",
                        data = null,
                    };
                    return Conflict(conflictResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<SettingViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var setting = new Setting
                {
                    Id = id,
                    Name = settingViewModel.Name,
                    Code = settingViewModel.Code,
                    Value = settingViewModel.Value,
                };

                _settingRepository.UpdateSetting(setting, updatedBy);

                settingViewModel.Id = id;
                var response = new DtoResponse<SettingViewModel>
                {
                    status = 200,
                    message = "Setting data saved successfully",
                    data = settingViewModel,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<SettingViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteSetting(string id)
        {
            try
            {
                var existingSetting = _settingRepository.GetSettingById(id);
                if (existingSetting == null)
                {
                    var notFoundResponse = new DtoResponse<object>
                    {
                        status = 404,
                        message = "Setting not found",
                        data = null,
                    };
                    return NotFound(notFoundResponse);
                }

                var deleteResult = _settingRepository.DeleteSetting(id);

                if (deleteResult == "success")
                {
                    var successResponse = new DtoResponse<object>
                    {
                        status = 200,
                        message = "Setting data deleted successfully",
                        data = null,
                    };
                    return Ok(successResponse);
                }
                else
                {
                    if (deleteResult.Contains("Foreign Key Conflict"))
                    {
                        var conflictResponse = new DtoResponse<object>
                        {
                            status = 409,
                            message =
                                "Cannot delete setting because it is referenced by other data. Please remove all references first.",
                            data = null,
                        };
                        return Conflict(conflictResponse);
                    }
                    else
                    {
                        var badRequestResponse = new DtoResponse<object>
                        {
                            status = 500,
                            message = $"Internal server error: {deleteResult}",
                            data = null,
                        };
                        return StatusCode(500, badRequestResponse);
                    }
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

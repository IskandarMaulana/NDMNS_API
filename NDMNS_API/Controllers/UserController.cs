using Microsoft.AspNetCore.Mvc;
using NDMNS_API.Models;
using NDMNS_API.Repositories;
using NDMNS_API.Responses;

namespace NDMNS_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<DtoResponse<List<UserViewModel>>> GetAllUsers()
        {
            try
            {
                var result = _userRepository.GetAllUsers();

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
                var response = new DtoResponse<List<UserViewModel>>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpGet("{id}")]
        public ActionResult<DtoResponse<UserViewModel>> GetUserById(string id)
        {
            try
            {
                var result = _userRepository.GetUserById(id);

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
                var response = new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPost]
        public ActionResult<DtoResponse<UserViewModel>> CreateUser(
            [FromBody] UserViewModel userViewModel
        )
        {
            try
            {
                if (
                    userViewModel == null
                    || string.IsNullOrWhiteSpace(userViewModel.Name)
                    || string.IsNullOrWhiteSpace(userViewModel.Nrp)
                    || string.IsNullOrWhiteSpace(userViewModel.Password)
                    || string.IsNullOrWhiteSpace(userViewModel.Code)
                    || string.IsNullOrWhiteSpace(userViewModel.Role)
                )
                {
                    var badRequestResponse = new DtoResponse<UserViewModel>
                    {
                        status = 400,
                        message =
                            "Invalid user data. Name, Code, Nrp, Password, and Role are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var createdBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(createdBy) || createdBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<UserViewModel>
                    {
                        status = 401,
                        message = "User not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var user = new User
                {
                    Name = userViewModel.Name,
                    Code = userViewModel.Code,
                    Nrp = userViewModel.Nrp,
                    Password = userViewModel.Password,
                    Role = userViewModel.Role,
                    Email = userViewModel.Email,
                    WhatsApp = userViewModel.WhatsApp,
                    WhatsAppClient = userViewModel.WhatsAppClient,
                    Status = userViewModel.Status,
                };

                var result = _userRepository.AddUser(user, createdBy);

                if (result.status == 201)
                {
                    return Created("", result);
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
                var response = new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("{id}")]
        public ActionResult<DtoResponse<UserViewModel>> UpdateUser(
            string id,
            [FromBody] UserViewModel userViewModel
        )
        {
            try
            {
                if (
                    userViewModel == null
                    || string.IsNullOrWhiteSpace(userViewModel.Name)
                    || string.IsNullOrWhiteSpace(userViewModel.Nrp)
                    || string.IsNullOrWhiteSpace(userViewModel.Code)
                    || string.IsNullOrWhiteSpace(userViewModel.Role)
                )
                {
                    var badRequestResponse = new DtoResponse<UserViewModel>
                    {
                        status = 400,
                        message = "Invalid user data. Name, Code, Nrp, and Role are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<UserViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var user = new User
                {
                    Id = userViewModel.Id,
                    Name = userViewModel.Name,
                    Code = userViewModel.Code,
                    Nrp = userViewModel.Nrp,
                    Password = userViewModel.Password,
                    Role = userViewModel.Role,
                    Email = userViewModel.Email,
                    WhatsApp = userViewModel.WhatsApp,
                    WhatsAppClient = userViewModel.WhatsAppClient,
                    Status = userViewModel.Status,
                };

                var result = _userRepository.UpdateUser(id, user, updatedBy);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 404)
                {
                    return NotFound(result);
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
                var response = new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpPut("password/{id}")]
        public ActionResult<DtoResponse<UserViewModel>> UpdateUserPassword(
            string id,
            [FromBody] UserViewModel userViewModel
        )
        {
            try
            {
                if (
                    userViewModel == null
                    || string.IsNullOrWhiteSpace(userViewModel.Name)
                    || string.IsNullOrWhiteSpace(userViewModel.Nrp)
                    || string.IsNullOrWhiteSpace(userViewModel.Code)
                    || string.IsNullOrWhiteSpace(userViewModel.Role)
                )
                {
                    var badRequestResponse = new DtoResponse<UserViewModel>
                    {
                        status = 400,
                        message = "Invalid user data. Name, Code, Nrp, and Role are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var updatedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(updatedBy) || updatedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<UserViewModel>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var user = new User
                {
                    Id = userViewModel.Id,
                    Name = userViewModel.Name,
                    Code = userViewModel.Code,
                    Nrp = userViewModel.Nrp,
                    Password = userViewModel.Password,
                    Role = userViewModel.Role,
                    Email = userViewModel.Email,
                    WhatsApp = userViewModel.WhatsApp,
                    WhatsAppClient = userViewModel.WhatsAppClient,
                    Status = userViewModel.Status,
                };

                var result = _userRepository.UpdateUserPassword(id, user, updatedBy);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 404)
                {
                    return NotFound(result);
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
                var response = new DtoResponse<UserViewModel>
                {
                    status = 500,
                    message = $"Internal server error: {ex.Message}",
                    data = null,
                };
                return StatusCode(500, response);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult<DtoResponse<object>> DeleteUser(string id)
        {
            try
            {
                var deletedBy = Request.Headers["X-User-Id"].ToString();

                if (string.IsNullOrWhiteSpace(deletedBy) || deletedBy == "System")
                {
                    var unauthorizedResponse = new DtoResponse<object>
                    {
                        status = 401,
                        message = "User session not found. Please login first.",
                        data = null,
                    };
                    return Unauthorized(unauthorizedResponse);
                }

                var result = _userRepository.DeleteUser(id);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 404)
                {
                    return NotFound(result);
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

        [HttpPost("login")]
        public ActionResult<DtoResponse<UserViewModel>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (
                    request == null
                    || string.IsNullOrWhiteSpace(request.Nrp)
                    || string.IsNullOrWhiteSpace(request.Password)
                )
                {
                    var badRequestResponse = new DtoResponse<UserViewModel>
                    {
                        status = 400,
                        message = "Nrp and Password are required.",
                        data = null,
                    };
                    return BadRequest(badRequestResponse);
                }

                var result = _userRepository.Login(request);

                if (result.status == 200)
                {
                    return Ok(result);
                }
                else if (result.status == 401)
                {
                    return Unauthorized(result);
                }
                else if (result.status == 403)
                {
                    return StatusCode(403, result);
                }
                else
                {
                    return StatusCode(result.status, result);
                }
            }
            catch (Exception ex)
            {
                var response = new DtoResponse<UserViewModel>
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

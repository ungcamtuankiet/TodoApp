using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MRT.Application.Interfaces;
using MRT.Contract.DTOs.User;
using MRT.Contracts.Abstractions.Shared;

namespace MRT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create-new-user")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateUserDTO user)
        {
            var result = new Result<object>();

            if (user == null)
            {
                result.Error = 1;
                result.Message = "User data is required.";
                return BadRequest(result);
            }

            try
            {
                var createdUser = await _userService.CreateNewUser(user);

                result.Error = 0;
                result.Message = "Tạo người dùng thành công.";
                result.Data = createdUser;
                return Ok(result);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx &&
                                               (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                string message = sqlEx.Message;

                if (message.Contains("Email"))
                    result.Message = "Email đã tồn tại.";
                else if (message.Contains("UserName"))
                    result.Message = "Tên người dùng đã tồn tại.";
                else if (message.Contains("PhoneNumber"))
                    result.Message = "Số điện thoại đã tồn tại.";
                else
                    result.Message = "Thông tin người dùng bị trùng.";

                result.Error = 1;
                return Conflict(result);
            }
            catch (Exception ex)
            {
                result.Error = 1;
                result.Message = "Lỗi hệ thống: " + ex.Message;
                return StatusCode(500, result);
            }
        }

        [HttpGet("get-current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var getUser = await _userService.GetCurrentUserById();
            return Ok(getUser);
        }
    }
}

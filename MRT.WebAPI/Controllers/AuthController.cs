using Google.Apis.Auth.OAuth2.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MRT.Application.Interfaces;
using MRT.Contract.DTOs.Auth;
using MRT.Contracts.Abstractions.Shared;

namespace MRT.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("user/login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            try
            {
                var token = await _authService.LoginAsync(loginDTO);
                Response.Cookies.Append("refreshToken", token.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = "/",
                    SameSite = SameSiteMode.Strict,
                });
                return Ok(new Result<object>
                {
                    Error = 0,
                    Message = "Success",
                    Data = new
                    {
                        TokenType = "Bearer",
                        token.AccessToken,
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpPost("token/refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var checkRefeshToken = await _authService.RefreshToken(request.RefreshToken);

                Response.Cookies.Append("refreshToken", checkRefeshToken.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = "/",
                    SameSite = SameSiteMode.Strict,
                });

                return Ok(new Result<object>
                {
                    Error = 0,
                    Message = "Success",
                    Data = new
                    {
                        TokenType = "Bearer",
                        checkRefeshToken.AccessToken
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpPost("user/logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] Guid userId)
        {
            await _authService.DeleteRefreshToken(userId);
            Response.Cookies.Delete("refreshToken");
            return Ok(new Result<object>
            {
                Error = 0,
                Message = "Logout Successfully",
                Data = null
            });
        }
    }
}

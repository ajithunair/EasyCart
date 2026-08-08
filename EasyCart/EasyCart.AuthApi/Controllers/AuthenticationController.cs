using EasyCart.AuthApi.DTOs;
using EasyCart.AuthApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EasyCart.AuthApi.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthenticationController(IUser userInterface) : Controller
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Response>> Register(AppUserDto appUserDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await userInterface.Register(appUserDto);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await userInterface.Login(
                loginDto,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

            if(result == null)
            {
                return BadRequest(new AuthResponseDto(false, "Invalid credentials", null));
            }

            if (result.Success)
            {
                SetRefreshTokenCookie(result.TokenPair!.RefreshToken);
                return Ok(result);
            }
            else
                return Unauthorized("Invalid credentials");
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var rawRefreshToken) ||
                string.IsNullOrWhiteSpace(rawRefreshToken))
            {
                return Unauthorized(new AuthResponseDto(false, "Refresh token is missing", null));
            }

            var result = await userInterface.Refresh(
                rawRefreshToken,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

            if (result is null)
            {
                Response.Cookies.Delete("refreshToken", RefreshTokenCookieOptions());
                return Unauthorized(new AuthResponseDto(false, "Invalid refresh token", null));
            }

            SetRefreshTokenCookie(result.TokenPair!.RefreshToken);
            return Ok(result);
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var rawRefreshToken) &&
                !string.IsNullOrWhiteSpace(rawRefreshToken))
            {
                await userInterface.Logout(rawRefreshToken);
            }

            Response.Cookies.Delete("refreshToken", RefreshTokenCookieOptions());
            return NoContent();
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<GetUserDto>> GetUser(int id)
        {
            if (id <= 0) return BadRequest("Invalid user Id");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") &&
                (!int.TryParse(currentUserId, out var authenticatedUserId) || authenticatedUserId != id))
            {
                return Forbid();
            }

            var user = await userInterface.GetUser(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = RefreshTokenCookieOptions();
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(2);
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

        private CookieOptions RefreshTokenCookieOptions() => new()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = HttpContext.Request.IsHttps,
            Path = "/api/Authentication"
        };
    }
}

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
        public async Task<ActionResult<Response>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await userInterface.Login(loginDto);
            if (result.Success)
            {
                return Ok(result);
            }
            else
                return BadRequest(result);
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
    }
}

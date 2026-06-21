using EasyCart.AuthApi.DTOs;
using EasyCart.AuthApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyCart.AuthApi.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController(IUser userInterface) : Controller
    {
        [HttpPost("register")]
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
            var user = await userInterface.GetUser(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}

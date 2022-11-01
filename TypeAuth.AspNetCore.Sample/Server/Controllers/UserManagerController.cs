using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TypeAuth.AspNetCore.Sample.Server.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserManagerDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagerController : ControllerBase
    {
        private readonly IUserManagerService userManagerService;

        public UserManagerController(IUserManagerService userManagerService)
        {
            this.userManagerService = userManagerService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            var token= await userManagerService.LoginAsync(login);

            if (token is null)
                return BadRequest();

            return Ok(token);
        }
    }
}

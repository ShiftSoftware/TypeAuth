using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;
using TypeAuth.AspNetCore.Sample.Server.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TypeAuth.AspNetCore.Sample.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IUserService userService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IUserService userService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userService = userService;
        }

        // GET: api/<UsersController>
        [HttpGet]
        public async Task<List<UserDto>> Get()
        {
            return await userService.GetUsersAsync();
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await userService.GetUserAsync(id);

            if (user is null)
                return NotFound();

            return Ok(mapper.Map<UserDto>(user));
        }

        // POST api/<UsersController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RegisterUserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await userService.RegisterUserAsync(userDto);

            await unitOfWork.SaveChangesAsync();

            return Ok(mapper.Map<UserDto>(user));
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RegisterUserDto userDto)
        {
            var user =await  userService.GetUserAsync(id);

            if (user is null)
                return NotFound();

            userService.UpdateUserAsync(user, userDto);

            await unitOfWork.SaveChangesAsync();

            return Ok(mapper.Map<UserDto>(user));
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await userService.GetUserAsync(id);

            if (user is null)
                return NotFound();

            userService.RemoveUser(user);

            await unitOfWork.SaveChangesAsync();

            return Ok(mapper.Map<UserDto>(user));
        }
    }
}

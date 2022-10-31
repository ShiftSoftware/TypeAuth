using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TypeAuth.AspNetCore.Sample.Server.ActionTreeModels;
using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TypeAuth.AspNetCore.Sample.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IRoleRepo roleRepo;
        private readonly IMapper mapper;

        public RolesController(IUnitOfWork unitOfWork, IRoleRepo roleRepo, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.roleRepo = roleRepo;
            this.mapper = mapper;
        }

        // GET: api/<RoleController>
        [HttpGet]
        public async Task<List<RoleDto>> Get()
        {
            return await roleRepo.GetRolesAsync();
        }

        // GET api/<RoleController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var role= await roleRepo.GetRoleAsync(id);

            if(role is null)
                return NotFound();

            return Ok(mapper.Map<RoleDto>(mapper.Map<RoleModel>(role)));
        }

        // POST api/<RoleController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UpdateRoleDto roleDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var role=await roleRepo.CrateRoleAsync(mapper.Map<Role>(mapper.Map<UpdateRoleModel>(roleDto)));

            await unitOfWork.SaveChangesAsync();

            return Ok(mapper.Map<RoleDto>(mapper.Map<RoleModel>(role)));
        }

        // PUT api/<RoleController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UpdateRoleDto roleDto)
        {
            var role = await roleRepo.GetRoleAsync(id);

            if (role is null)
                return NotFound();

            var roleModel = mapper.Map<UpdateRoleModel>(roleDto);
            mapper.Map(roleModel, role);

            await unitOfWork.SaveChangesAsync();

            return Ok(mapper.Map<RoleDto>(mapper.Map<RoleModel>(role)));
        }

        // DELETE api/<RoleController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await roleRepo.GetRoleAsync(id);

            if (role is null)
                return NotFound();

            roleRepo.RemoveRole(role);

            await unitOfWork.SaveChangesAsync();

            return Ok(mapper.Map<RoleDto>(mapper.Map<RoleModel>(role)));
        }
    }
}

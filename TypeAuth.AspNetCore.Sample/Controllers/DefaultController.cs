using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ShiftSoftware.TypeAuth.AspNetCore.Services;
using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Shared.ActionTrees;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShiftSoftware.TypeAuth.AspNetCore.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DefaultController : ControllerBase
    {
        private readonly ITypeAuthService typeAuthService;
        IConfiguration configuration;

        public DefaultController(ITypeAuthService typeAuthService, IConfiguration configuration)
        {
            this.typeAuthService = typeAuthService;
            this.configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public ActionResult Auth(List<string> accessTrees)
        {
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]!);

            var claims = new List<Claim>
            {
                new Claim("Id", Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (accessTrees.Count() > 0)
                foreach (var accessTree in accessTrees)
                    claims.Add(new Claim(TypeAuthClaimTypes.AccessTree, accessTree));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var stringToken = tokenHandler.WriteToken(token);

            return Ok(stringToken);
        }

        [HttpGet("read")]
        [TypeAuth<CRMActions>(nameof(CRMActions.Tickets), ShiftSoftware.TypeAuth.Core.Access.Read)]
        public ActionResult Read()
        {
            return Ok();
        }

        [HttpGet("read-nested")]
        [TypeAuth<SystemActions.UserModule>(nameof(SystemActions.UserModule.Users), ShiftSoftware.TypeAuth.Core.Access.Read)]
        public ActionResult ReadNested()
        {
            return Ok();
        }

        [HttpGet("write")]
        [TypeAuth<CRMActions>(nameof(CRMActions.Tickets), ShiftSoftware.TypeAuth.Core.Access.Write)]
        public ActionResult Write()
        {
            return Ok();
        }

        [HttpGet("delete")]
        [TypeAuth<CRMActions>(nameof(CRMActions.Tickets), ShiftSoftware.TypeAuth.Core.Access.Delete)]
        public ActionResult Delete()
        {
            return Ok();
        }

        [HttpGet("can-access-city/{key}")]
        public ActionResult CanAccessDepartment(string key)
        {
            if (this.typeAuthService.CanAccess<DataLevel>(x => x.Cities, key))
                return Ok();

            return Forbid();
        }
    }
}

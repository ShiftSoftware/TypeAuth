using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Server.Services.Interfaces;

namespace TypeAuth.AspNetCore.Sample.Server.Services
{
    public class TokenService : ITokenService
    {
        public string GenerateJwtToken(User user, int expireAfterDays = 30)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
            };

            //Set claims for user roles and access trees
            foreach (var role in user.UserInRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
                claims.Add(new Claim(TypeAuthClaimTypes.AccessTree, role.Role.AccessTree));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("VerySecretKeyVerySecretKeyVerySecretKeyVerySecretKeyVerySecretKey"));

            var token = new JwtSecurityToken(
                issuer: "TypeAuth",
                audience: "TypeAuth",
                claims: claims,
                expires: DateTime.Now.AddDays(expireAfterDays),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature));

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }
    }
}

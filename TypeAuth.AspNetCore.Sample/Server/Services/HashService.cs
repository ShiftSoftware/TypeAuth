using System.Security.Cryptography;
using System.Text;
using TypeAuth.AspNetCore.Sample.Server.General;
using TypeAuth.AspNetCore.Sample.Server.Services.Interfaces;

namespace TypeAuth.AspNetCore.Sample.Server.Services
{
    public class HashService : IHashService
    {
        public HashModel GenerateHash(string password)
        {
            HashModel result = new();

            using (var hmac = new HMACSHA512())
            {
                result.Salt = hmac.Key;
                result.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            return result;
        }

        public bool VerifyPassword(string password, byte[] salt, byte[] passwordHash)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                var generatedPassowrdHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                return generatedPassowrdHash.SequenceEqual(passwordHash);
            }
        }
    }
}

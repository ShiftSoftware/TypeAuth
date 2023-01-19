using System.Text;
using System.Text.Json;

namespace TypeAuth.AspNetCore.Tests
{
    internal class Common
    {
        public static async Task<string> GetTokenAsync(HttpClient _client, List<dynamic> accessTrees)
        {
            var stringifiedAccessTrees = accessTrees.Select(x => JsonSerializer.Serialize(x));

            var httpContent = new StringContent(JsonSerializer.Serialize(stringifiedAccessTrees), Encoding.UTF8, "application/json");

            HttpResponseMessage obj = await _client.PostAsync("/api/default/token", httpContent);

            var token = await obj.Content.ReadAsStringAsync();

            return token;
        }
    }
}

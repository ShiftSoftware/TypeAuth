using ShiftSoftware.TypeAuth.Core;
using TypeAuth.AspNetCore.Sample;
using Xunit.Abstractions;

namespace TypeAuth.AspNetCore.Tests.Tests
{
    [Collection("API Collection")]
    public class DynamicAction
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper Output;

        public DynamicAction(CustomWebApplicationFactory<WebMarker> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            Output = output;
        }

        [Fact(DisplayName = "01.Data Level Access")]
        public async Task _01_DataLevelAccess()
        {
            var token = await Common.GetTokenAsync(_client, new List<dynamic> { });
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            var response = await this._client.GetAsync($"/api/default/can-access-city/_1");
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);

            _client.DefaultRequestHeaders.Remove("Authorization");

            token = await Common.GetTokenAsync(_client, new List<dynamic>
            {
                new {
                    DataLevel = new
                    {
                        Cities = new { 
                            _1 = new List<Access> { Access.Read }
                        }
                    }
                }
            });
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            response = await this._client.GetAsync($"/api/default/can-access-city/_1");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            response = await this._client.GetAsync($"/api/default/can-access-city/_2");
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}

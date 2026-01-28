using ShiftSoftware.TypeAuth.AspNetCore.Sample;
using ShiftSoftware.TypeAuth.Core;

namespace ShiftSoftware.TypeAuth.AspNetCore.Tests.Tests
{
    [Collection("API Collection")]
    public class TypeAuthAttribute
    {
        private readonly HttpClient _client;
        private readonly ITestOutputHelper Output;

        public TypeAuthAttribute(CustomWebApplicationFactory<WebMarker> factory, ITestOutputHelper output)
        {
            _client = factory.CreateClient();
            Output = output;
        }

        [Fact(DisplayName = "01. Read Access")]
        public async Task _01_ReadAccess()
        {
            var token = await Common.GetTokenAsync(_client, new List<dynamic>
            {
                new {
                    CRMActions = new {
                        Tickets = new List<Access> { Access.Read }
                    }
                }
            });

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/read", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, (await _client.GetAsync("/api/default/write", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, (await _client.GetAsync("/api/default/delete", TestContext.Current.CancellationToken)).StatusCode);
        }

        [Fact(DisplayName = "02. Read/Write Access")]
        public async Task _02_ReadWriteAccess()
        {
            var token = await Common.GetTokenAsync(_client, new List<dynamic>
            {
                new {
                    CRMActions = new {
                        Tickets = new List<Access> {
                            Access.Read,
                            Access.Write
                        }
                    }
                }
            });

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/read", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/write", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.Forbidden, (await _client.GetAsync("/api/default/delete", TestContext.Current.CancellationToken)).StatusCode);
        }

        [Fact(DisplayName = "03. Read/Write/Delete Access")]
        public async Task _03_ReadWriteDeleteAccess()
        {
            var token = await Common.GetTokenAsync(_client, new List<dynamic>
            {
                new {
                    CRMActions = new {
                        Tickets = new List<Access> {
                            Access.Read,
                            Access.Write,
                            Access.Delete
                        }
                    }
                }
            });

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/read", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/write", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/delete", TestContext.Current.CancellationToken)).StatusCode);
        }

        [Fact(DisplayName = "04. Multiple Access Trees")]
        public async Task _04_MultipleAccessTrees()
        {
            var token = await Common.GetTokenAsync(_client, new List<dynamic>
            {
                new {
                    CRMActions = new {
                        Tickets = new List<Access> {
                            Access.Read
                        }
                    }
                },
                new {
                    CRMActions = new {
                        Tickets = new List<Access> {
                            Access.Write
                        }
                    }
                },
                new {
                    CRMActions = new {
                        Tickets = new List<Access> {
                            Access.Delete
                        }
                    }
                }
            });

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/read", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/write", TestContext.Current.CancellationToken)).StatusCode);

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/delete", TestContext.Current.CancellationToken)).StatusCode);
        }

        [Fact(DisplayName = "05. Read Nested")]
        public async Task _05_ReadNested()
        {
            var token = await Common.GetTokenAsync(_client, new List<dynamic>
            {
                new {
                    SystemActions = new {
                        UserModule = new List<Access> { Access.Read }
                    }
                }
            });

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            Assert.Equal(System.Net.HttpStatusCode.OK, (await _client.GetAsync("/api/default/read-nested", TestContext.Current.CancellationToken)).StatusCode);
        }
    }
}

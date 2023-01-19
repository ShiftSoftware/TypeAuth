using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using ShiftSoftware.TypeAuth.AspNetCore.Sample;

namespace ShiftSoftware.TypeAuth.AspNetCore.Tests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            var host = builder.Build();
            
            host.Start();

            return host;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            //builder
            //    .ConfigureServices(services =>
            //    {
            //        var config = new ConfigurationBuilder()
            //       .SetBasePath(AppContext.BaseDirectory)
            //       .AddJsonFile("appsettings.json", false, true)
            //       .Build();
            //    });
        }
    }

    [CollectionDefinition("API Collection")]
    public class DatabaseCollection : ICollectionFixture<CustomWebApplicationFactory<WebMarker>>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}


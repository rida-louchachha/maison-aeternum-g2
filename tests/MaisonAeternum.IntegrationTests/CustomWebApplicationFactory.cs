using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MaisonAeternum.IntegrationTests;

// Points the app at a dedicated Postgres database instead of the dev one, so integration tests
// never touch (or get polluted by) whatever data is sitting in the developer's own database.
// A real Postgres instance (not EF Core InMemory) is used deliberately — Program.cs runs
// Database.MigrateAsync() at startup, which the InMemory provider does not support.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Host=localhost;Port=5433;Database=MaisonAeternumTests;Username=postgres;Password=postgres"
            });
        });
    }

    public HttpClient CreateHttpsClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
}

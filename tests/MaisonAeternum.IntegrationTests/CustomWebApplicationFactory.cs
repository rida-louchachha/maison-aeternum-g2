using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace MaisonAeternum.IntegrationTests;

/// <summary>
/// Points the app at a dedicated LocalDB database instead of the dev one, so integration tests
/// never touch (or get polluted by) whatever data is sitting in the developer's own database.
/// Real SQL Server (not EF Core InMemory) is used deliberately — Program.cs runs
/// Database.MigrateAsync() at startup, which the InMemory provider does not support.
/// </summary>
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
                    "Server=(localdb)\\MSSQLLocalDB;Database=MaisonAeternumTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
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

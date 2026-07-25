using MaisonAeternum.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MaisonAeternum.UnitTests;

/// <summary>Each call returns a fresh, isolated in-memory database — no shared state between tests.</summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}

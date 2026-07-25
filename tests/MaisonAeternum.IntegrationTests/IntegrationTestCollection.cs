namespace MaisonAeternum.IntegrationTests;

/// <summary>
/// All integration test classes share ONE CustomWebApplicationFactory instance (and therefore one
/// migration/seed run against the test LocalDB database) instead of each spinning up their own —
/// xUnit runs different test classes in parallel by default, and concurrent app startups racing
/// to migrate/seed the same database produce spurious duplicate-key/missing-index failures that
/// have nothing to do with application correctness.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}

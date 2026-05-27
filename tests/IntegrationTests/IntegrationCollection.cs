namespace IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<CrowdNfoApiFactory>
{
    public const string Name = "Integration";
}

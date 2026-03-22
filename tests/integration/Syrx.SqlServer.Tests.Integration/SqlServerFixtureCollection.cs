namespace Syrx.SqlServer.Tests.Integration
{
    [CollectionDefinition(nameof(SqlServerFixtureCollection))]
    public class SqlServerFixtureCollection : ICollectionFixture<SqlServerFixture> { }
}

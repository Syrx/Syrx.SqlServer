namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerQueryAsync(SqlServerFixture fixture) : QueryAsync(fixture) { }
}

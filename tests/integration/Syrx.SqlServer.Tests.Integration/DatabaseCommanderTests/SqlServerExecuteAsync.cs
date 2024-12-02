namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerExecuteAsync(SqlServerFixture fixture) : ExecuteAsync(fixture) { }
}

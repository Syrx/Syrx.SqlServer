namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerExecute(SqlServerFixture fixture) : Execute(fixture) { }
}

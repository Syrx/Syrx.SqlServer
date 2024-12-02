namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerQuery(SqlServerFixture fixture) : Query(fixture) { }
}

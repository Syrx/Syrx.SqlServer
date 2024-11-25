namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerDispose(SqlServerFixture fixture) : Dispose(fixture) { }
}

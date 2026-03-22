namespace Syrx.SqlServer.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerDispose(SqlServerFixture fixture) : Dispose(fixture) { }
}

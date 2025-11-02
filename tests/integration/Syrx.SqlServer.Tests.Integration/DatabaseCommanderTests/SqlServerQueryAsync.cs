using Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests;

namespace Syrx.SqlServer.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerQueryAsync(SqlServerFixture fixture) : QueryAsync(fixture) { }
}

using Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests;

namespace Syrx.SqlServer.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerQuery(SqlServerFixture fixture) : Query(fixture) { }
}

using Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests;
using System.Transactions;

namespace Syrx.SqlServer.Tests.Integration.DatabaseCommanderTests
{
    [Collection(nameof(SqlServerFixtureCollection))]
    public class SqlServerExecute(SqlServerFixture fixture) : Execute(fixture)
    {
        // SQL Server supports ambient transactions, so no need to skip the test as in MySQL.
    }
}

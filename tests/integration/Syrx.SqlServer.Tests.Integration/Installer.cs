using Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests;
using Syrx.Commanders.Databases.Connectors.SqlServer.Extensions;

namespace Syrx.SqlServer.Tests.Integration
{
    public class Installer
    {
        public static IServiceProvider Install(string alias, string connectionString)
        {
            return new ServiceCollection()
                .UseSyrx(factory => factory
                    .SetupSqlServer(alias, connectionString))
                .BuildServiceProvider();
        }

        public static void SetupDatabase(ICommander<DatabaseBuilder> commander)
        {
            var builder = new DatabaseBuilder(commander);
            builder.Build();
        }
    }
}

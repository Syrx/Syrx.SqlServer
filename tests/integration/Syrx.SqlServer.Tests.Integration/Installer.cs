namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
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

    //public class DatabaseCreator
    //{
    //    public DatabaseCreator()
    //    {
    //        var database = DatabaseOptionsBuilderExtensions.Build(a => a
    //            .WithName("syrx")
    //                .AddTable(b => b
    //                    .WithName("poco")
    //                    .AddField(c => c
    //                        .WithName("id")
    //                        .WithDataType(SqlDbType.Int)
    //                        .IsNullable(false)
    //                        .IsIdentity(true)
    //                        )
    //                    .AddField(c => c
    //                        .WithName("name")
    //                        .WithDataType(SqlDbType.NVarChar)
    //                        .HasWidth(50)
    //                        )
    //                    .AddField(c => c
    //                        .WithName("value")
    //                        .WithDataType(SqlDbType.UniqueIdentifier)
    //                        )
    //                    )
    //                .AddTable(b => b
    //                    .WithName("bulk_insert"))
    //                );
    //    }
    //}
}

namespace Syrx.SqlServer.Tests.Integration
{
    public class SqlServerInstaller 
    {
        public IServiceProvider Provider { get; }

        public SqlServerInstaller(string connectionString)
        {
            var services = new ServiceCollection();
            var builder = new SyrxBuilder(services);
            _ = builder.SetupSqlServer(connectionString);
                        
            Provider = services.BuildServiceProvider();
            var commander = Provider.GetService<ICommander<DatabaseBuilder>>();
            var database = new DatabaseBuilder(commander);
            database.Build();
        }
    }

}

namespace Syrx.SqlServer.Tests.Integration
{
    public class SqlServerInstaller 
    {
        public SyrxBuilder SyrxBuilder { get; }
        public IServiceProvider Provider { get; }

        public SqlServerInstaller(string connectionString)
        {
            var services = new ServiceCollection();
            var builder = new SyrxBuilder(services);
            SyrxBuilder = builder.SetupSqlServer(connectionString);
                        
            Provider = services.BuildServiceProvider();
            var commander = Provider.GetService<ICommander<DatabaseBuilder>>();
            var database = new DatabaseBuilder(commander);
            database.Build();
        }
    }

    public static class SyrxInstaller
    {
        public static IServiceCollection Install(this IServiceCollection services)
        {
            return services.UseSyrx(factory =>
                factory.UseSqlServer(builder =>
                    builder
                        .AddConnectionStrings()
                        .AddSetupBuilderOptions()
                ));
        }
    }
}

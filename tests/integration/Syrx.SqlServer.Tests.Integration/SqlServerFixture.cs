using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    public class SqlServerFixture : Fixture, IAsyncLifetime
    {
        private readonly MsSqlContainer _container;

        public SqlServerFixture()
        {
            var _logger = LoggerFactory.Create(b => b
                .AddConsole()
                .AddSystemdConsole()
                .AddSimpleConsole()).CreateLogger<SqlServerFixture>();

            _container = new MsSqlBuilder()
                .WithLogger(_logger)
                .WithReuse(true)
                .WithStartupCallback((container, token) =>
                {
                    var message = @$"{new string('=', 150)}
Syrx: {nameof(MsSqlContainer)} startup callback. Container details:
{new string('=', 150)}
Name ............. : {container.Name}
Id ............... : {container.Id}
State ............ : {container.State}
Health ........... : {container.Health}
CreatedTime ...... : {container.CreatedTime}
StartedTime ...... : {container.StartedTime}
Hostname ......... : {container.Hostname}
Image.Digest ..... : {container.Image.Digest}
Image.FullName ... : {container.Image.FullName}
Image.Registry ... : {container.Image.Registry}
Image.Repository . : {container.Image.Repository}
Image.Tag ........ : {container.Image.Tag}
IpAddress ........ : {container.IpAddress}
MacAddress ....... : {container.MacAddress}
ConnectionString . : {container.GetConnectionString()}
{new string('=', 150)}
";
                    container.Logger.LogInformation(message);
                    return Task.CompletedTask;
                }).Build();

            // start
            _container.StartAsync().Wait();
        }

        public async Task DisposeAsync()
        {
            await Task.Run(() => Console.WriteLine("Done"));
        }

        public async Task InitializeAsync()
        {
            // line up
            var connectionString = _container.GetConnectionString();
            var alias = "Syrx.Sql";

            var provider = Installer.Install(alias, connectionString);

            // call Install() on the base type. 
            Install(() => Installer.Install(alias, connectionString));
            Installer.SetupDatabase(base.ResolveCommander<DatabaseBuilder>());

            // set assertions for Execute message
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsTransactionRollback),
                $"Arithmetic overflow error converting expression to data type float.{Environment.NewLine}The statement has been terminated.");


            await Task.CompletedTask;
        }

    }
}

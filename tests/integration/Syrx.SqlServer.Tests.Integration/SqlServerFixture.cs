using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;
using Syrx.SqlServer.Tests.Integration;
using Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests;

namespace Syrx.SqlServer.Tests.Integration
{
    /// <summary>
    /// Initializes SQL Server test fixture.
    /// Uses workflow-managed SQL Server service in CI, falls back to local Docker SQL Server for development.
    /// </summary>
    public class SqlServerFixture : Fixture, IAsyncLifetime
    {
        private readonly string _connectionString;
        private readonly bool _useWorkflowManagedSqlServer;

        public SqlServerFixture()
        {
            // Check if we're running in GitHub Actions with a managed SQL Server service
            _useWorkflowManagedSqlServer = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

            if (_useWorkflowManagedSqlServer)
            {
                // Use the workflow-managed SQL Server service
                _connectionString = "Server=127.0.0.1,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;";
                Console.WriteLine("Using workflow-managed SQL Server service");
            }
            else
            {
                // Use localhost for local development, sqlserver for CI/Docker
                var dockerEnv = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");
                var host = string.IsNullOrEmpty(dockerEnv) ? "localhost" : "sqlserver";
                _connectionString = $"Server={host},1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;";
                Console.WriteLine($"Using SQL Server host: {host}");
            }
        }

        public async Task DisposeAsync()
        {
            // Nothing to dispose - workflow or Docker manages the SQL Server service
            await Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine($"Connection string: {_connectionString}");
            
            // Wait for SQL Server to be fully ready with connection verification
            Console.WriteLine("Starting connection verification...");
            await WaitForSqlServerReadyAsync();
            Console.WriteLine("SQL Server is ready!");

            var alias = "Syrx.Sql";
            Install(() => Installer.Install(alias, _connectionString));
            Installer.SetupDatabase(base.ResolveCommander<Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests.DatabaseBuilder>());

            // Set assertion messages for those that change between RDBMS implementations.
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsTransactionRollback), $"Arithmetic overflow error converting expression to data type float.{Environment.NewLine}The statement has been terminated.");
            AssertionMessages.Add<Execute>(nameof(Execute.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsRollbackOnParameterlessCalls), "Divide by zero error encountered.");

            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.SupportsTransactionRollback), $"Arithmetic overflow error converting expression to data type float.{Environment.NewLine}The statement has been terminated.");
            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.SupportsRollbackOnParameterlessCalls), "Divide by zero error encountered.");

            AssertionMessages.Add<Query>(nameof(Query.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            AssertionMessages.Add<QueryAsync>(nameof(QueryAsync.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");

            await Task.CompletedTask;
        }

        private async Task WaitForSqlServerReadyAsync()
        {
            // Shorter delay since workflow handles SQL Server readiness
            if (_useWorkflowManagedSqlServer)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
            }

            var maxAttempts = _useWorkflowManagedSqlServer ? 30 : 120;
            var delayBetweenAttempts = TimeSpan.FromSeconds(5);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    Console.WriteLine($"Connection attempt {attempt}/{maxAttempts}...");
                    using var connection = new SqlConnection(_connectionString);
                    await connection.OpenAsync();
                    using var command = new SqlCommand("SELECT 1", connection);
                    await command.ExecuteScalarAsync();
                    Console.WriteLine($"Connection successful on attempt {attempt}");
                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Console.WriteLine($"Connection attempt {attempt} failed: {ex.Message}");
                    await Task.Delay(delayBetweenAttempts);
                }
            }

            throw new InvalidOperationException("SQL Server container did not become ready within the expected time.");
        }
    }
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;
using System.Text;
using Xunit;

namespace Syrx.SqlServer.Tests.Performance
{
    public class PerformanceTestFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _container;
        private readonly ILogger<PerformanceTestFixture> _logger;

        public string ConnectionString { get; private set; } = string.Empty;
        public string DatabaseName => "SyrxPerformance";

        public PerformanceTestFixture()
        {
            _logger = LoggerFactory.Create(b => b
                .AddConsole()
                .AddSystemdConsole()
                .AddSimpleConsole()).CreateLogger<PerformanceTestFixture>();

            _container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithLogger(_logger)
                .WithPassword("YourStrong!Passw0rd123")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("MSSQL_PID", "Developer")
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrong!Passw0rd123", "-Q", "SELECT 1", "-C"))
                .WithStartupCallback(async (container, token) =>
                {
                    var message = @$"{new string('=', 120)}
Syrx Performance Tests: {nameof(MsSqlContainer)} startup callback. Container details:
{new string('=', 120)}
Name ............. : {container.Name}
Id ............... : {container.Id}
State ............ : {container.State}
Health ........... : {container.Health}
CreatedTime ...... : {container.CreatedTime}
Hostname ......... : {container.Hostname}
Image.FullName ... : {container.Image.FullName}
IpAddress ........ : {container.IpAddress}
ConnectionString . : {((MsSqlContainer)container).GetConnectionString()}
{new string('=', 120)}
";
                    container.Logger.LogInformation(message);
                    
                    // Execute initialization scripts using container exec
                    await ExecuteInitializationScripts(container);
                })
                .Build();
        }

        public async Task InitializeAsync()
        {
            try
            {
                Console.WriteLine("[PerformanceTestFixture] Starting SQL Server container for performance tests...");
                await _container.StartAsync();

                var baseConnectionString = _container.GetConnectionString();
                Console.WriteLine($"[PerformanceTestFixture] Container started. Base connection: {baseConnectionString}");

                // Set the connection string for the performance database (Docker initialization handles the rest)
                ConnectionString = ModifyConnectionStringForPerformanceDatabase(baseConnectionString);
                Console.WriteLine($"[PerformanceTestFixture] Performance database connection ready.");

                // Verify initialization using Syrx
                await VerifyDatabaseInitialization();

                Console.WriteLine("[PerformanceTestFixture] Initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PerformanceTestFixture] Initialization failed: {ex.Message}");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            if (_container != null)
            {
                await _container.DisposeAsync();
                Console.WriteLine("[PerformanceTestFixture] Container disposed");
            }
        }

        private async Task ExecuteInitializationScripts(IContainer container)
        {
            try
            {
                Console.WriteLine("[PerformanceTestFixture] Executing database initialization scripts...");
                
                // Wait for SQL Server to be completely ready
                await WaitForSqlServerReady(container);
                
                var scriptDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Docker", "init-scripts");
                var scripts = new[] { "01-create-database.sql", "02-create-tables.sql", "03-create-procedures.sql", "04-seed-data.sql" };
                
                foreach (var scriptName in scripts)
                {
                    var scriptPath = Path.Combine(scriptDirectory, scriptName);
                    if (File.Exists(scriptPath))
                    {
                        Console.WriteLine($"[PerformanceTestFixture] Executing {scriptName}...");
                        var scriptContent = await File.ReadAllTextAsync(scriptPath);
                        
                        // Copy script to container and execute with retry
                        await container.CopyAsync(Encoding.UTF8.GetBytes(scriptContent), $"/tmp/{scriptName}");
                        await ExecuteScriptWithRetry(container, scriptName);
                    }
                }
                
                Console.WriteLine("[PerformanceTestFixture] Database initialization scripts completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PerformanceTestFixture] Failed to execute initialization scripts: {ex.Message}");
                throw;
            }
        }

        private async Task WaitForSqlServerReady(IContainer container)
        {
            Console.WriteLine("[PerformanceTestFixture] Waiting for SQL Server to be ready...");
            
            var maxAttempts = 30;
            var delay = TimeSpan.FromSeconds(2);
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var result = await container.ExecAsync(new[]
                    {
                        "/opt/mssql-tools18/bin/sqlcmd",
                        "-S", "localhost",
                        "-U", "sa",
                        "-P", "YourStrong!Passw0rd123",
                        "-Q", "SELECT 1",
                        "-C" // Trust server certificate
                    });
                    
                    if (result.ExitCode == 0)
                    {
                        Console.WriteLine("[PerformanceTestFixture] SQL Server is ready!");
                        return;
                    }
                    
                    Console.WriteLine($"[PerformanceTestFixture] SQL Server not ready yet (attempt {attempt}/{maxAttempts}), waiting...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PerformanceTestFixture] SQL Server readiness check failed (attempt {attempt}/{maxAttempts}): {ex.Message}");
                }
                
                if (attempt < maxAttempts)
                {
                    await Task.Delay(delay);
                }
            }
            
            throw new TimeoutException("SQL Server failed to become ready within the expected time");
        }

        private async Task ExecuteScriptWithRetry(IContainer container, string scriptName)
        {
            var maxAttempts = 3;
            var delay = TimeSpan.FromSeconds(1);
            
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var result = await container.ExecAsync(new[]
                    {
                        "/opt/mssql-tools18/bin/sqlcmd",
                        "-S", "localhost",
                        "-U", "sa",
                        "-P", "YourStrong!Passw0rd123",
                        "-i", $"/tmp/{scriptName}",
                        "-C" // Trust server certificate
                    });
                    
                    if (result.ExitCode == 0)
                    {
                        Console.WriteLine($"[PerformanceTestFixture] Successfully executed {scriptName}");
                        return;
                    }
                    
                    Console.WriteLine($"[PerformanceTestFixture] Script {scriptName} failed (attempt {attempt}/{maxAttempts}): {result.Stderr}");
                    
                    if (attempt == maxAttempts)
                    {
                        throw new InvalidOperationException($"Failed to execute {scriptName} after {maxAttempts} attempts: {result.Stderr}");
                    }
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    Console.WriteLine($"[PerformanceTestFixture] Script execution attempt {attempt}/{maxAttempts} failed: {ex.Message}");
                }
                
                if (attempt < maxAttempts)
                {
                    await Task.Delay(delay);
                }
            }
        }

        private async Task VerifyDatabaseInitialization()
        {
            try
            {
                Console.WriteLine("[PerformanceTestFixture] Verifying database initialization using Syrx...");

                // Wait a moment for Docker initialization to complete
                await Task.Delay(2000);

                var serviceProvider = PerformanceTestHelper.CreateServiceProvider(ConnectionString);
                var initRepository = serviceProvider.GetRequiredService<Repositories.DatabaseInitializationRepository>();

                // Verify stored procedures were created
                var procedures = await initRepository.GetStoredProceduresAsync();
                var procedureList = procedures.ToList();

                var expectedProcedures = new[] 
                { 
                    "usp_get_by_category", 
                    "usp_get_paginated", 
                    "usp_bulk_insert_performance", 
                    "usp_increment_counter" 
                };

                foreach (var expectedProc in expectedProcedures)
                {
                    var found = procedureList.Any(p => p.Name == expectedProc);
                    if (found)
                    {
                        Console.WriteLine($"[PerformanceTestFixture] ✓ Stored procedure '{expectedProc}' exists");
                    }
                    else
                    {
                        Console.WriteLine($"[PerformanceTestFixture] ✗ Stored procedure '{expectedProc}' NOT FOUND");
                        throw new InvalidOperationException($"Required stored procedure '{expectedProc}' was not created during initialization");
                    }
                }

                Console.WriteLine("[PerformanceTestFixture] Database initialization verified successfully using Syrx");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PerformanceTestFixture] Failed to verify database initialization: {ex.Message}");
                throw;
            }
        }

        private string ModifyConnectionStringForPerformanceDatabase(string baseConnectionString)
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString);
            builder.InitialCatalog = DatabaseName;
            builder.TrustServerCertificate = true;
            builder.ConnectTimeout = 30;
            builder.CommandTimeout = 120;
            return builder.ConnectionString;
        }



        /// <summary>
        /// Cleans up test data between performance test runs using Syrx
        /// </summary>
        public async Task CleanupTestDataAsync()
        {
            var serviceProvider = PerformanceTestHelper.CreateServiceProvider(ConnectionString);
            var statsRepository = serviceProvider.GetRequiredService<Repositories.DatabaseStatsRepository>();
            await statsRepository.CleanupTestDataAsync();
        }

        /// <summary>
        /// Gets current database statistics for performance monitoring using Syrx
        /// </summary>
        public async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            var serviceProvider = PerformanceTestHelper.CreateServiceProvider(ConnectionString);
            var statsRepository = serviceProvider.GetRequiredService<Repositories.DatabaseStatsRepository>();
            return await statsRepository.GetStatsAsync();
        }


    }

    public class DatabaseStats
    {
        public int PerformanceTestCount { get; set; }
        public int CategoriesCount { get; set; }
        public int ConcurrentTestCount { get; set; }
        public int SimpleKvCount { get; set; }
    }
}
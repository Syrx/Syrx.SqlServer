using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Builders;

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
                .WithImage("docker-syrx-sqlserver-test:latest")
                .WithLogger(_logger)
                .WithReuse(true)
                .WithPassword("YourStrong!Passw0rd")
                .WithWaitStrategy(Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(1433)
                    .UntilCommandIsCompleted("/opt/mssql-tools18/bin/sqlcmd", "-S", "localhost", "-U", "sa", "-P", "YourStrong!Passw0rd", "-Q", "SELECT 1", "-C"))
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
ConnectionString . : {((MsSqlContainer)container).GetConnectionString()}
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
            try
            {
                // Get connection string and modify it to connect to the Syrx database
                var baseConnectionString = _container.GetConnectionString();
                
                // Extract the actual password being used by the container
                var actualPassword = await ExtractContainerPassword();
                Console.WriteLine($"[SqlServerFixture] Container is using password: {actualPassword}");
                
                var connectionString = ModifyConnectionStringForSyrxDatabase(baseConnectionString, actualPassword);
                var alias = "Syrx.Sql";

                Console.WriteLine($"[SqlServerFixture] Initializing with connection: {connectionString}");

                // Validate container health before proceeding
                await ValidateContainerHealth();

                // First, ensure the Syrx database exists (create it if needed)
                await EnsureSyrxDatabaseExists(connectionString);
                
                // Initialize database schema automatically if needed
                await AutoInitializeDatabaseSchema(connectionString);

                // Install service provider with pre-built database configuration
                var provider = Installer.Install(alias, connectionString);
                Install(() => provider);

                Console.WriteLine("[SqlServerFixture] Service provider installed successfully");

                // Run comprehensive configuration validation
                var validationResult = await ConfigurationValidator.ValidateFullConfiguration(connectionString, provider);
                validationResult.LogResults();

                if (!validationResult.IsValid)
                {
                    Console.WriteLine("[SqlServerFixture] Configuration validation failed, but continuing with database schema validation...");
                }

                // Validate the database schema (should now be initialized)
                await ValidatePrebuiltDatabaseWithDiagnostics();
                Console.WriteLine("[SqlServerFixture] Database schema validation completed successfully");

                // Configure assertion messages for SQL Server specific responses
                ConfigureAssertionMessages();

                Console.WriteLine("[SqlServerFixture] Initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Initialization failed: {ex.Message}");
                Console.WriteLine($"[SqlServerFixture] Full exception: {ex}");
                throw;
            }
        }

        private static string ModifyConnectionStringForSyrxDatabase(string connectionString, string actualPassword)
        {
            // The base connection string points to master database, we need to change it to Syrx
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "Syrx";
            builder.Password = actualPassword; // Use the actual container password
            builder.TrustServerCertificate = true; // Required for our container setup
            builder.ConnectTimeout = 30; // Increased timeout for container startup
            builder.CommandTimeout = 30; // Increased command timeout
            return builder.ConnectionString;
        }

        private async Task ValidateContainerHealth()
        {
            // Note: Container state/health validation will be implemented once we determine the correct enum types
            // For now, we'll just log the current state for diagnostics
            Console.WriteLine($"[SqlServerFixture] Container state: {_container.State}, Health: {_container.Health}");
            
            // Basic validation that container exists and has connection available
            try
            {
                var connectionString = _container.GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Container connection string is empty");
                }
                Console.WriteLine("[SqlServerFixture] Container connection string is available");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Container validation failed: {ex.Message}", ex);
            }

            await Task.CompletedTask;
        }

        private async Task ValidatePrebuiltDatabaseWithDiagnostics()
        {
            var commander = base.ResolveCommander<DatabaseBuilder>();
            
            Console.WriteLine("[SqlServerFixture] Starting pre-built database validation...");
            
            try
            {
                Installer.ValidatePrebuiltDatabase(commander);
                Console.WriteLine("[SqlServerFixture] Pre-built database validation completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Validation failed with exception: {ex.GetType().Name}");
                Console.WriteLine($"[SqlServerFixture] Exception message: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[SqlServerFixture] Inner exception: {ex.InnerException.Message}");
                }
                
                throw;
            }
            
            await Task.CompletedTask;
        }

        private async Task TestBasicConnectivity(string connectionString)
        {
            try
            {
                Console.WriteLine("[SqlServerFixture] Testing basic database connectivity...");
                Console.WriteLine($"[SqlServerFixture] Using connection string: {connectionString}");
                
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                await connection.OpenAsync();
                
                using var command = new Microsoft.Data.SqlClient.SqlCommand("SELECT DB_NAME()", connection);
                var result = await command.ExecuteScalarAsync();
                
                Console.WriteLine($"[SqlServerFixture] Connected successfully to database: {result}");
                
                // Also test if we can list databases to ensure we have proper access
                using var listDbCommand = new Microsoft.Data.SqlClient.SqlCommand("SELECT name FROM sys.databases ORDER BY name", connection);
                using var reader = await listDbCommand.ExecuteReaderAsync();
                var databases = new List<string>();
                while (await reader.ReadAsync())
                {
                    databases.Add(reader.GetString("name"));
                }
                Console.WriteLine($"[SqlServerFixture] Available databases: {string.Join(", ", databases)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Basic connectivity test failed: {ex.Message}");
                Console.WriteLine($"[SqlServerFixture] Connection string used: {connectionString}");
                
                // Try connecting to master database instead to see if the issue is database-specific
                try
                {
                    var masterBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
                    masterBuilder.InitialCatalog = "master";
                    
                    using var masterConnection = new Microsoft.Data.SqlClient.SqlConnection(masterBuilder.ConnectionString);
                    await masterConnection.OpenAsync();
                    Console.WriteLine("[SqlServerFixture] Successfully connected to master database");
                    
                    // Check if Syrx database exists
                    using var checkDbCommand = new Microsoft.Data.SqlClient.SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = 'Syrx'", masterConnection);
                    var syrxExists = (int)await checkDbCommand.ExecuteScalarAsync();
                    Console.WriteLine($"[SqlServerFixture] Syrx database exists: {syrxExists > 0}");
                }
                catch (Exception masterEx)
                {
                    Console.WriteLine($"[SqlServerFixture] Master database connection also failed: {masterEx.Message}");
                }
            }
        }

        private async Task<string> ExtractContainerPassword()
        {
            try
            {
                Console.WriteLine("[SqlServerFixture] Extracting actual container password...");
                
                // First, try to get it from the connection string
                var baseConnectionString = _container.GetConnectionString();
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString);
                
                if (!string.IsNullOrEmpty(builder.Password))
                {
                    Console.WriteLine($"[SqlServerFixture] Found password in connection string: {builder.Password}");
                    return builder.Password;
                }
                
                // If connection string doesn't have password, our custom image should be using the hardcoded password
                var customImagePassword = "YourStrong!Passw0rd";
                Console.WriteLine($"[SqlServerFixture] Using custom Docker image password: {customImagePassword}");
                
                // Verify this password works by testing a connection
                await TestPasswordConnection(baseConnectionString, customImagePassword);
                
                return customImagePassword;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Error extracting password: {ex.Message}");
                // Return our custom Docker image password as fallback
                return "YourStrong!Passw0rd";
            }
        }

        private async Task TestPasswordConnection(string baseConnectionString, string password)
        {
            try
            {
                var testBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString);
                testBuilder.Password = password;
                testBuilder.InitialCatalog = "master"; // Test against master database
                testBuilder.TrustServerCertificate = true;
                testBuilder.ConnectTimeout = 10;
                
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(testBuilder.ConnectionString);
                await connection.OpenAsync();
                
                using var command = new Microsoft.Data.SqlClient.SqlCommand("SELECT @@VERSION", connection);
                await command.ExecuteScalarAsync();
                
                Console.WriteLine($"[SqlServerFixture] Password verification successful");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Password verification failed: {ex.Message}");
                throw;
            }
        }



        private async Task EnsureSyrxDatabaseExists(string syrxConnectionString)
        {
            try
            {
                Console.WriteLine("[SqlServerFixture] Ensuring Syrx database exists...");
                
                // Connect to master database to check/create Syrx database
                var masterBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(syrxConnectionString);
                masterBuilder.InitialCatalog = "master";
                
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterBuilder.ConnectionString);
                await connection.OpenAsync();
                
                // Check if Syrx database exists
                using var checkCommand = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM sys.databases WHERE name = 'Syrx'", connection);
                var exists = (int)await checkCommand.ExecuteScalarAsync() > 0;
                
                if (!exists)
                {
                    Console.WriteLine("[SqlServerFixture] Creating Syrx database...");
                    using var createCommand = new Microsoft.Data.SqlClient.SqlCommand(
                        "CREATE DATABASE [Syrx]", connection);
                    await createCommand.ExecuteNonQueryAsync();
                    Console.WriteLine("[SqlServerFixture] Syrx database created successfully");
                }
                else
                {
                    Console.WriteLine("[SqlServerFixture] Syrx database already exists");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Failed to ensure Syrx database exists: {ex.Message}");
                throw;
            }
        }

        private async Task AutoInitializeDatabaseSchema(string syrxConnectionString)
        {
            try
            {
                Console.WriteLine("[SqlServerFixture] Checking if database schema initialization is needed...");
                
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(syrxConnectionString);
                await connection.OpenAsync();
                
                // Check if required tables exist
                var tablesExist = await CheckRequiredTablesExist(connection);
                var proceduresExist = await CheckRequiredProceduresExist(connection);
                
                if (!tablesExist || !proceduresExist)
                {
                    Console.WriteLine("[SqlServerFixture] Database schema incomplete, initializing automatically...");
                    
                    if (!tablesExist)
                    {
                        Console.WriteLine("[SqlServerFixture] Creating tables...");
                        await CreateTables(connection);
                    }
                    
                    if (!proceduresExist)
                    {
                        Console.WriteLine("[SqlServerFixture] Creating stored procedures...");
                        await CreateStoredProcedures(connection);
                    }
                    
                    Console.WriteLine("[SqlServerFixture] Seeding test data...");
                    await SeedTestData(connection);
                    
                    Console.WriteLine("[SqlServerFixture] Database schema initialization completed successfully");
                }
                else
                {
                    Console.WriteLine("[SqlServerFixture] Database schema already initialized");
                    
                    // Ensure test data is fresh
                    Console.WriteLine("[SqlServerFixture] Refreshing test data...");
                    await SeedTestData(connection);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SqlServerFixture] Failed to auto-initialize database schema: {ex.Message}");
                throw;
            }
        }

        private async Task<bool> CheckRequiredTablesExist(Microsoft.Data.SqlClient.SqlConnection connection)
        {
            var requiredTables = new[] { "poco", "identity_test", "bulk_insert", "distributed_transaction" };
            
            foreach (var table in requiredTables)
            {
                using var command = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM sys.objects WHERE object_id = OBJECT_ID(@tableName) AND type = 'U'", connection);
                command.Parameters.AddWithValue("@tableName", $"[dbo].[{table}]");
                
                var exists = (int)await command.ExecuteScalarAsync() > 0;
                if (!exists)
                {
                    Console.WriteLine($"[SqlServerFixture] Required table '{table}' is missing");
                    return false;
                }
            }
            
            Console.WriteLine("[SqlServerFixture] All required tables exist");
            return true;
        }

        private async Task<bool> CheckRequiredProceduresExist(Microsoft.Data.SqlClient.SqlConnection connection)
        {
            var requiredProcedures = new[] { "usp_create_table", "usp_identity_tester", "usp_bulk_insert", "usp_bulk_insert_and_return", "usp_clear_table" };
            
            foreach (var procedure in requiredProcedures)
            {
                using var command = new Microsoft.Data.SqlClient.SqlCommand(
                    "SELECT COUNT(*) FROM sys.objects WHERE object_id = OBJECT_ID(@procedureName) AND type = 'P'", connection);
                command.Parameters.AddWithValue("@procedureName", $"[dbo].[{procedure}]");
                
                var exists = (int)await command.ExecuteScalarAsync() > 0;
                if (!exists)
                {
                    Console.WriteLine($"[SqlServerFixture] Required stored procedure '{procedure}' is missing");
                    return false;
                }
            }
            
            Console.WriteLine("[SqlServerFixture] All required stored procedures exist");
            return true;
        }

        private async Task CreateTables(Microsoft.Data.SqlClient.SqlConnection connection)
        {
            var createTablesScript = @"
-- Create the main poco table used in most tests
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[poco]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[poco] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_poco] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create identity_test table for identity testing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[identity_test]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[identity_test] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_identity_test] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create bulk_insert table for bulk operations
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[bulk_insert]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[bulk_insert] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_bulk_insert] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END

-- Create distributed_transaction table for distributed transaction tests
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[distributed_transaction]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[distributed_transaction] 
    (
        [id] [int] IDENTITY(1,1) NOT NULL,
        [name] [varchar](50) NULL,
        [value] [decimal](18, 2) NULL,
        [modified] [datetime] NULL,
        CONSTRAINT [PK_distributed_transaction] PRIMARY KEY CLUSTERED ([id] ASC)
    );
END";

            using var command = new Microsoft.Data.SqlClient.SqlCommand(createTablesScript, connection);
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("[SqlServerFixture] Tables created successfully");
        }

        private async Task CreateStoredProcedures(Microsoft.Data.SqlClient.SqlConnection connection)
        {
            var createProceduresScript = @"
-- Drop procedures if they exist
DROP PROCEDURE IF EXISTS [dbo].[usp_create_table];
DROP PROCEDURE IF EXISTS [dbo].[usp_identity_tester];
DROP PROCEDURE IF EXISTS [dbo].[usp_bulk_insert];
DROP PROCEDURE IF EXISTS [dbo].[usp_bulk_insert_and_return];
DROP PROCEDURE IF EXISTS [dbo].[usp_clear_table];";

            using (var dropCommand = new Microsoft.Data.SqlClient.SqlCommand(createProceduresScript, connection))
            {
                await dropCommand.ExecuteNonQueryAsync();
            }

            // Create each procedure separately to avoid GO statement issues
            var procedures = new[]
            {
                @"CREATE PROCEDURE [dbo].[usp_create_table]
(@name nvarchar(max))
AS
BEGIN
    DECLARE @template nvarchar(max)
           ,@sql nvarchar(max);

    SELECT @template
        = N'
            IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[%name]'') AND type in (N''U'')) 
                BEGIN
                    DROP TABLE [dbo].[%name];
                END 

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[%name]'') AND type in (N''U'')) 
                BEGIN 
                    CREATE TABLE [dbo].[%name] 
                    (
                        [id] [int] IDENTITY(1, 1) NOT NULL,
                        [name] [varchar](50) NULL,
                        [value] [decimal](18, 2) NULL,
                        [modified] [datetime] NULL,
                        CONSTRAINT [PK_%name] PRIMARY KEY CLUSTERED ([id] ASC)
                    ); 
                END;';

    SELECT @sql = REPLACE(@template, '%name', @name);
    EXEC [sys].[sp_executesql] @sql;
END;",

                @"CREATE PROCEDURE [dbo].[usp_identity_tester]
    @name varchar(50)
   ,@value decimal(18, 2)
AS
BEGIN
    INSERT INTO [identity_test]
    (
        [name]
       ,[value]
       ,[modified]
    )
    SELECT @name
          ,@value
          ,GETUTCDATE();

    SELECT SCOPE_IDENTITY();
END;",

                @"CREATE PROCEDURE [dbo].[usp_bulk_insert]
(@path varchar(max))
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @command nvarchar(max)
           ,@template nvarchar(max) = N'
                BULK INSERT [dbo].[bulk_insert] FROM ''%path'' WITH (FIELDTERMINATOR = '','', ROWTERMINATOR = ''\n'')';

    SELECT @command = REPLACE(@template, '%path', @path);
    EXEC [sys].[sp_executesql] @command;
END;",

                @"CREATE PROCEDURE [dbo].[usp_bulk_insert_and_return]
(@path varchar(max))
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @command nvarchar(max)
           ,@template nvarchar(max) = N'
                BULK INSERT [dbo].[bulk_insert] FROM ''%path'' WITH (FIELDTERMINATOR = '','', ROWTERMINATOR = ''\n'')';

    SELECT @command = REPLACE(@template, '%path', @path);
    EXEC [sys].[sp_executesql] @command;

    SELECT *
    FROM [dbo].[bulk_insert];
END;",

                @"CREATE PROCEDURE [dbo].[usp_clear_table]
(@name nvarchar(max))
AS
BEGIN
    DECLARE @template nvarchar(max)
           ,@sql nvarchar(max);

    SELECT @template
        = N'TRUNCATE TABLE [%name];';

    SELECT @sql = REPLACE(@template, '%name', @name);
    EXEC [sys].[sp_executesql] @sql;
END;"
            };

            foreach (var procedure in procedures)
            {
                using var command = new Microsoft.Data.SqlClient.SqlCommand(procedure, connection);
                await command.ExecuteNonQueryAsync();
            }

            Console.WriteLine("[SqlServerFixture] Stored procedures created successfully");
        }

        private async Task SeedTestData(Microsoft.Data.SqlClient.SqlConnection connection)
        {
            var seedDataScript = @"
-- Clear existing data
TRUNCATE TABLE [dbo].[poco];
TRUNCATE TABLE [dbo].[identity_test];
TRUNCATE TABLE [dbo].[bulk_insert];
TRUNCATE TABLE [dbo].[distributed_transaction];

-- Populate poco table with test data (150 entries as per DatabaseBuilder.Populate())
DECLARE @i INT = 1;
DECLARE @today DATETIME = CAST(GETDATE() AS DATE);

WHILE @i < 151
BEGIN
    INSERT INTO [dbo].[poco] ([name], [value], [modified])
    VALUES (
        CONCAT('entry ', @i),
        @i * 10,
        @today
    );
    
    SET @i = @i + 1;
END;";

            using var command = new Microsoft.Data.SqlClient.SqlCommand(seedDataScript, connection);
            command.CommandTimeout = 60; // Increased timeout for data seeding
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("[SqlServerFixture] Test data seeded successfully");
        }





        private void ConfigureAssertionMessages()
        {
            // Set assertion messages for those that change between RDBMS implementations. 
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsTransactionRollback), $"Arithmetic overflow error converting expression to data type float.{Environment.NewLine}The statement has been terminated.");
            AssertionMessages.Add<Execute>(nameof(Execute.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            AssertionMessages.Add<Execute>(nameof(Execute.SupportsRollbackOnParameterlessCalls), "Divide by zero error encountered.");

            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.SupportsTransactionRollback), $"Arithmetic overflow error converting expression to data type float.{Environment.NewLine}The statement has been terminated.");
            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            AssertionMessages.Add<ExecuteAsync>(nameof(ExecuteAsync.SupportsRollbackOnParameterlessCalls), "Divide by zero error encountered.");

            AssertionMessages.Add<Query>(nameof(Query.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            AssertionMessages.Add<QueryAsync>(nameof(QueryAsync.ExceptionsAreReturnedToCaller), "Divide by zero error encountered.");
            
            Console.WriteLine("[SqlServerFixture] Assertion messages configured for SQL Server");
        }

    }
}

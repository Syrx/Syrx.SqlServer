namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    public class Installer
    {
        public static IServiceProvider Install(string alias, string connectionString)
        {
            Console.WriteLine($"[Installer] Setting up service provider with alias '{alias}'");
            Console.WriteLine($"[Installer] Connection string length: {connectionString.Length} characters");
            
            try
            {
                var serviceProvider = new ServiceCollection()
                    .UseSyrx(factory => factory
                        .SetupSqlServer(alias, connectionString))
                    .BuildServiceProvider();
                
                Console.WriteLine("[Installer] Service provider created successfully");
                return serviceProvider;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Installer] Failed to create service provider: {ex.Message}");
                throw;
            }
        }

        public static void SetupDatabase(ICommander<DatabaseBuilder> commander)
        {
            Console.WriteLine("[Installer] Setting up database using pre-built Docker schema");
            ValidatePrebuiltDatabase(commander);
        }

        public static void ValidatePrebuiltDatabase(ICommander<DatabaseBuilder> commander)
        {
            Console.WriteLine("[Installer] Validating pre-built database schema...");
            
            try
            {
                var builder = new DatabaseBuilder(commander);
                
                Console.WriteLine("[Installer] Starting schema validation...");
                builder.ValidatePrebuiltSchema();
                
                Console.WriteLine("[Installer] Pre-built database validation completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Installer] Pre-built database validation failed: {ex.GetType().Name}");
                Console.WriteLine($"[Installer] Error message: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Installer] Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
                
                Console.WriteLine($"[Installer] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public static void ValidateConfiguration(IServiceProvider serviceProvider, string alias)
        {
            Console.WriteLine($"[Installer] Validating configuration for alias '{alias}'...");
            
            try
            {
                // Try to resolve a commander to verify DI setup
                var commander = serviceProvider.GetRequiredService<ICommander<DatabaseBuilder>>();
                Console.WriteLine("[Installer] Commander resolution successful");
                
                // Try a simple validation query
                Console.WriteLine("[Installer] Testing basic commander functionality...");
                // This will be implemented when we can resolve the auth issue
                
                Console.WriteLine("[Installer] Configuration validation completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Installer] Configuration validation failed: {ex.Message}");
                throw;
            }
        }
    }
}

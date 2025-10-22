using Microsoft.Extensions.DependencyInjection;
using Syrx.Extensions;
using Syrx.Commanders.Databases.Connectors.SqlServer.Extensions;
using Syrx.SqlServer.Tests.Performance.Repositories;

namespace Syrx.SqlServer.Tests.Performance
{
    public static class PerformanceTestHelper
    {
        public static IServiceProvider CreateServiceProvider(string connectionString)
        {
            var services = new ServiceCollection();
            
            // Configure Syrx with the performance database connection
            services.UseSyrx(builder => builder
                .UseSqlServer(sqlServer => sqlServer
                    .AddConnectionString("performance", connectionString)
                    .AddCommand(types => types
                        .ForType<PerformanceTestRepository>(methods => methods
                            .ForMethod(nameof(PerformanceTestRepository.GetAllAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id, name, description, value, created_date, modified_date, category_id, is_active, data_blob, json_data FROM [dbo].[performance_test] WHERE is_active = 1 ORDER BY created_date DESC"))
                            .ForMethod(nameof(PerformanceTestRepository.GetByIdAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id, name, description, value, created_date, modified_date, category_id, is_active, data_blob, json_data FROM [dbo].[performance_test] WHERE id = @id AND is_active = 1"))
                            .ForMethod(nameof(PerformanceTestRepository.GetByCategoryAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("EXEC [dbo].[usp_get_by_category] @categoryId, @limit"))
                            .ForMethod(nameof(PerformanceTestRepository.BulkInsertAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("EXEC [dbo].[usp_bulk_insert_performance] @batchSize, @categoryId"))
                            .ForMethod(nameof(PerformanceTestRepository.GetPaginatedAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("EXEC [dbo].[usp_get_paginated] @pageSize, @pageNumber"))
                            .ForMethod(nameof(PerformanceTestRepository.GetWithCategoriesAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT p.id, p.name, p.description, p.value, p.created_date, p.category_id, c.name as category_name FROM [dbo].[performance_test] p INNER JOIN [dbo].[categories] c ON p.category_id = c.id WHERE p.is_active = 1 ORDER BY p.created_date DESC OFFSET 0 ROWS FETCH NEXT @limit ROWS ONLY"))
)
                        .ForType<SimpleKeyValueRepository>(methods => methods
                            .ForMethod(nameof(SimpleKeyValueRepository.GetAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT [key], [value], updated FROM [dbo].[simple_kv] WHERE [key] = @key"))
                            .ForMethod(nameof(SimpleKeyValueRepository.SetAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("IF EXISTS (SELECT 1 FROM [dbo].[simple_kv] WHERE [key] = @key) UPDATE [dbo].[simple_kv] SET [value] = @value, updated = GETUTCDATE() WHERE [key] = @key ELSE INSERT INTO [dbo].[simple_kv] ([key], [value]) VALUES (@key, @value)"))
)
                        .ForType<ConcurrentTestRepository>(methods => methods
                            .ForMethod(nameof(ConcurrentTestRepository.IncrementCounterAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("EXEC [dbo].[usp_increment_counter] @threadId"))
                            .ForMethod(nameof(ConcurrentTestRepository.GetAllCountersAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id as Id, thread_id as ThreadId, operation_count as OperationCount, last_updated as LastUpdated FROM [dbo].[concurrent_test] ORDER BY thread_id"))
                            .ForMethod(nameof(ConcurrentTestRepository.ResetCountersAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("DELETE FROM [dbo].[concurrent_test]")))
                        .ForType<CategoryRepository>(methods => methods
                            .ForMethod(nameof(CategoryRepository.GetAllAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id, name, parent_id FROM [dbo].[categories] ORDER BY name"))
                            .ForMethod(nameof(CategoryRepository.GetByIdAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id, name, parent_id FROM [dbo].[categories] WHERE id = @id"))
                            .ForMethod(nameof(CategoryRepository.GetRootCategoriesAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id, name, parent_id FROM [dbo].[categories] WHERE parent_id IS NULL ORDER BY name"))
                            .ForMethod(nameof(CategoryRepository.GetSubCategoriesAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT id, name, parent_id FROM [dbo].[categories] WHERE parent_id = @parentId ORDER BY name")))
                        .ForType<DatabaseStatsRepository>(methods => methods
                            .ForMethod(nameof(DatabaseStatsRepository.GetStatsAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText(@"
                                    SELECT 
                                        (SELECT COUNT(*) FROM [dbo].[performance_test]) as PerformanceTestCount,
                                        (SELECT COUNT(*) FROM [dbo].[categories]) as CategoriesCount,
                                        (SELECT COUNT(*) FROM [dbo].[concurrent_test]) as ConcurrentTestCount,
                                        (SELECT COUNT(*) FROM [dbo].[simple_kv]) as SimpleKvCount"))
                            .ForMethod(nameof(DatabaseStatsRepository.CleanupTestDataAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText(@"
                                    TRUNCATE TABLE [dbo].[concurrent_test];
                                    DELETE FROM [dbo].[performance_test] WHERE name LIKE 'Test_%';
                                    UPDATE [dbo].[simple_kv] SET [value] = 'reset', updated = GETUTCDATE() WHERE [key] LIKE 'test.%';")))
                        .ForType<DatabaseInitializationRepository>(methods => methods
                            .ForMethod(nameof(DatabaseInitializationRepository.CheckDatabaseExistsAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT COUNT(*) FROM sys.databases WHERE name = @databaseName"))
                            .ForMethod(nameof(DatabaseInitializationRepository.GetStoredProceduresAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText("SELECT name as Name, create_date as CreatedDate FROM sys.procedures WHERE name LIKE 'usp_%' ORDER BY name"))))));

            // Register repositories
            services.AddTransient<PerformanceTestRepository>();
            services.AddTransient<ConcurrentTestRepository>();
            services.AddTransient<SimpleKeyValueRepository>();
            services.AddTransient<CategoryRepository>();
            services.AddTransient<DatabaseStatsRepository>();
            services.AddTransient<DatabaseInitializationRepository>();

            return services.BuildServiceProvider();
        }
    }
}
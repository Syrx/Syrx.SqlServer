using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using Syrx.Extensions;
using Syrx.Commanders.Databases;
using Syrx.Commanders.Databases.Settings;
using Syrx.Commanders.Databases.Connectors.SqlServer.Extensions;
using Syrx.SqlServer.Tests.Performance.Models;
using Syrx.SqlServer.Tests.Performance.Repositories;
using System.ComponentModel;

namespace Syrx.SqlServer.Tests.Performance.Benchmarks
{
    /// <summary>
    /// Comprehensive benchmark testing different record counts across all CommandFlagSetting combinations.
    /// Tests 1, 10, 100, 1000, and 10000 record selections with various flag configurations.
    /// </summary>
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 3)]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MarkdownExporter, HtmlExporter]
    public class CommandFlagSettingsBenchmarks
    {
        private PerformanceTestFixture _fixture = null!;
        private Dictionary<CommandFlagSetting, CommandFlagSettingRepository> _repositories = null!;

        [Params(1, 1000)] // reduced to 1 and 1000 for quicker benchmarks
        public int RecordCount { get; set; }

        [Params(
            CommandFlagSetting.None,
            CommandFlagSetting.Buffered,
            CommandFlagSetting.Pipelined, 
            CommandFlagSetting.NoCache,
            CommandFlagSetting.Buffered | CommandFlagSetting.NoCache,
            CommandFlagSetting.Buffered | CommandFlagSetting.Pipelined,
            CommandFlagSetting.Pipelined | CommandFlagSetting.NoCache,
            CommandFlagSetting.Buffered | CommandFlagSetting.Pipelined | CommandFlagSetting.NoCache
        )]
        public CommandFlagSetting Flags { get; set; }

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _fixture = new PerformanceTestFixture();
            await _fixture.InitializeAsync();

            _repositories = new Dictionary<CommandFlagSetting, CommandFlagSettingRepository>();

            // Create repositories for the specified CommandFlagSetting combinations
            var flagCombinations = new[]
            {
                CommandFlagSetting.None,
                CommandFlagSetting.Buffered,
                CommandFlagSetting.Pipelined, 
                CommandFlagSetting.NoCache,
                CommandFlagSetting.Buffered | CommandFlagSetting.NoCache,
                CommandFlagSetting.Buffered | CommandFlagSetting.Pipelined,
                CommandFlagSetting.Pipelined | CommandFlagSetting.NoCache,
                CommandFlagSetting.Buffered | CommandFlagSetting.Pipelined | CommandFlagSetting.NoCache
            };
            
            foreach (var flagSetting in flagCombinations)
            {
                var serviceProvider = CommandFlagSettingTestHelper.CreateServiceProvider(_fixture.ConnectionString, flagSetting);
                var repository = serviceProvider.GetRequiredService<CommandFlagSettingRepository>();
                _repositories[flagSetting] = repository;
            }
        }

        [GlobalCleanup]
        public async Task GlobalCleanup()
        {
            await _fixture.DisposeAsync();
            
            foreach (var serviceProvider in _repositories.Values.Select(r => r.ServiceProvider))
            {
                if (serviceProvider is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        [Benchmark]
        [Description("Select records with TOP clause using various CommandFlagSettings")]
        public async Task<int> SelectRecords_WithFlags()
        {
            var repository = _repositories[Flags];
            var results = await repository.SelectTopRecordsAsync(RecordCount);
            return results.Count();
        }

        [Benchmark]
        [Description("Select records by category with limit using various CommandFlagSettings")]
        public async Task<int> SelectByCategory_WithFlags()
        {
            var repository = _repositories[Flags];
            var results = await repository.SelectByCategoryAsync(1, RecordCount);
            return results.Count();
        }

        [Benchmark]
        [Description("Select records by date range with limit using various CommandFlagSettings")]
        public async Task<int> SelectByDateRange_WithFlags()
        {
            var repository = _repositories[Flags];
            var results = await repository.SelectByDateRangeAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, RecordCount);
            return results.Count();
        }


    }

    /// <summary>
    /// Repository specifically designed for CommandFlagSetting benchmarks
    /// </summary>
    public class CommandFlagSettingRepository
    {
        private readonly ICommander<CommandFlagSettingRepository> _commander;

        public CommandFlagSettingRepository(ICommander<CommandFlagSettingRepository> commander, IServiceProvider serviceProvider)
        {
            _commander = commander ?? throw new ArgumentNullException(nameof(commander));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Select top N records from PerformanceTestEntity
        /// </summary>
        public async Task<IEnumerable<PerformanceTestEntity>> SelectTopRecordsAsync(int count)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { count });

        /// <summary>
        /// Select records by category with limit
        /// </summary>
        public async Task<IEnumerable<PerformanceTestEntity>> SelectByCategoryAsync(int categoryId, int limit)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { categoryId, limit });

        /// <summary>
        /// Select records by date range with limit
        /// </summary>
        public async Task<IEnumerable<PerformanceTestEntity>> SelectByDateRangeAsync(DateTime startDate, DateTime endDate, int limit)
            => await _commander.QueryAsync<PerformanceTestEntity>(new { startDate, endDate, limit });
    }

    /// <summary>
    /// Helper class to create service providers with different CommandFlagSettings
    /// </summary>
    public static class CommandFlagSettingTestHelper
    {
        public static IServiceProvider CreateServiceProvider(string connectionString, CommandFlagSetting flagSetting)
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging();

            // Configure Syrx with specific CommandFlagSetting
            services.UseSyrx(builder => builder
                .UseSqlServer(options => options
                    .AddConnectionString("performance", connectionString)
                    .AddCommand(typeBuilder => typeBuilder
                        .ForType<CommandFlagSettingRepository>(commandBuilder => commandBuilder
                            
                            // SelectTopRecordsAsync command
                            .ForMethod(nameof(CommandFlagSettingRepository.SelectTopRecordsAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText(@"
                                    SELECT TOP (@count) 
                                        id, name, description, value, category_id, is_active, 
                                        created_date, modified_date, json_data 
                                    FROM [dbo].[performance_test] 
                                    ORDER BY id")
                                .SetFlags(flagSetting)
                                .SetCommandTimeout(30))
                            
                            // SelectByCategoryAsync command
                            .ForMethod(nameof(CommandFlagSettingRepository.SelectByCategoryAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText(@"
                                    SELECT TOP (@limit) 
                                        id, name, description, value, category_id, is_active, 
                                        created_date, modified_date, json_data 
                                    FROM [dbo].[performance_test] 
                                    WHERE category_id = @categoryId 
                                    ORDER BY id")
                                .SetFlags(flagSetting)
                                .SetCommandTimeout(30))
                            
                            // SelectByDateRangeAsync command  
                            .ForMethod(nameof(CommandFlagSettingRepository.SelectByDateRangeAsync), command => command
                                .UseConnectionAlias("performance")
                                .UseCommandText(@"
                                    SELECT TOP (@limit) 
                                        id, name, description, value, category_id, is_active, 
                                        created_date, modified_date, json_data 
                                    FROM [dbo].[performance_test] 
                                    WHERE created_date >= @startDate AND created_date <= @endDate 
                                    ORDER BY created_date DESC")
                                .SetFlags(flagSetting)
                                .SetCommandTimeout(30))
                        ))));

            // Register the repository
            services.AddTransient<CommandFlagSettingRepository>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Microsoft.Data.SqlClient;
using Syrx.Commanders.Databases.Settings;
using Syrx.Commanders.Databases.Settings.Extensions;
using Syrx.Commanders.Databases.Connectors.SqlServer;
using System.Data;

namespace Syrx.SqlServer.Benchmarks.Performance
{
    /// <summary>
    /// Comprehensive benchmarks comparing current Syrx implementation
    /// against baseline ADO.NET to measure performance characteristics
    /// </summary>
    [Config(typeof(PerformanceBenchmarkConfig))]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [CategoriesColumn]
    public class Phase3PerformanceBenchmarks
    {
        private ICommanderSettings _settings = null!;
        private SqlServerDatabaseConnector _syrxConnector = null!;
        private SqlServerDatabaseConnector _baselineConnector = null!;
        private readonly string _connectionString = "Server=(localdb)\\mssqllocaldb;Database=SyrxBenchmark;Trusted_Connection=true;";
        private List<TestDataModel> _testData = null!;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            // Initialize settings using the builder pattern
            _settings = CommanderSettingsBuilderExtensions.Build(builder => builder
                .AddConnectionString("benchmark", _connectionString));

            // Initialize connectors - both using the same real SqlServerDatabaseConnector
            _syrxConnector = new SqlServerDatabaseConnector(_settings);
            _baselineConnector = new SqlServerDatabaseConnector(_settings);

            // Setup test database and data
            await SetupTestDatabase();
            await SetupTestData();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // The SqlServerDatabaseConnector doesn't implement IDisposable in the current API
            // Connection management is handled by the underlying DatabaseConnector base class
        }

        #region Connection Management Benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Connection")]
        public Task<int> Baseline_RawADO_ConnectionCreation()
        {
            var totalQueries = 0;
            
            for (int i = 0; i < 10; i++)
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM BenchmarkData";
                var result = (int)command.ExecuteScalar()!;
                totalQueries++;
            }
            
            return Task.FromResult(totalQueries);
        }

        [Benchmark]
        [BenchmarkCategory("Connection")]
        public Task<int> Syrx_ConnectionCreation()
        {
            var totalQueries = 0;
            var commandSetting = new CommandSetting { 
                ConnectionAlias = "benchmark", 
                CommandText = "SELECT COUNT(*) FROM BenchmarkData" 
            };
            
            for (int i = 0; i < 10; i++)
            {
                using var connection = _syrxConnector.CreateConnection(commandSetting);
                connection.Open();
                
                using var command = connection.CreateCommand();
                command.CommandText = commandSetting.CommandText;
                var result = (int)command.ExecuteScalar()!;
                totalQueries++;
            }
            
            return Task.FromResult(totalQueries);
        }

        #endregion

        #region Statement Caching Benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("StatementCache")]
        public Task<int> Baseline_RepeatedQueries()
        {
            var totalResults = 0;
            var commandSetting = new CommandSetting { 
                ConnectionAlias = "benchmark", 
                CommandText = "SELECT Id, Name, Value FROM BenchmarkData WHERE Id = @id" 
            };
            
            using var connection = _baselineConnector.CreateConnection(commandSetting);
            connection.Open();

            for (int i = 0; i < 20; i++)
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name, Value FROM BenchmarkData WHERE Id = @id";
                
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = (i % 100) + 1;
                command.Parameters.Add(parameter);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    totalResults++;
                }
            }
            
            return Task.FromResult(totalResults);
        }

        [Benchmark]
        [BenchmarkCategory("StatementCache")]
        public Task<int> Syrx_RepeatedQueries()
        {
            var totalResults = 0;
            var commandSetting = new CommandSetting { 
                ConnectionAlias = "benchmark", 
                CommandText = "SELECT Id, Name, Value FROM BenchmarkData WHERE Id = @id" 
            };
            
            using var connection = _syrxConnector.CreateConnection(commandSetting);
            connection.Open();

            for (int i = 0; i < 20; i++)
            {
                using var command = connection.CreateCommand();
                command.CommandText = commandSetting.CommandText;
                
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@id";
                parameter.Value = (i % 100) + 1;
                command.Parameters.Add(parameter);
                
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    totalResults++;
                }
            }
            
            return Task.FromResult(totalResults);
        }

        #endregion

        #region Large Result Set Benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("LargeResultSet")]
        public async Task<int> Baseline_LargeResultSet()
        {
            var processedCount = 0;
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Value FROM BenchmarkData ORDER BY Id";
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new TestDataModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Value = reader.GetInt32(2)
                };
                processedCount++;
                
                // Simulate processing work
                if (processedCount % 100 == 0)
                {
                    await Task.Delay(1);
                }
            }
            
            return processedCount;
        }

        [Benchmark]
        [BenchmarkCategory("LargeResultSet")]
        public async Task<int> Syrx_LargeResultSet()
        {
            var processedCount = 0;
            var commandSetting = new CommandSetting { 
                ConnectionAlias = "benchmark", 
                CommandText = "SELECT Id, Name, Value FROM BenchmarkData ORDER BY Id" 
            };
            
            using var connection = _syrxConnector.CreateConnection(commandSetting);
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = commandSetting.CommandText;
            
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var item = new TestDataModel
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Value = reader.GetInt32(2)
                };
                processedCount++;
                
                // Simulate processing work
                if (processedCount % 100 == 0)
                {
                    await Task.Delay(1);
                }
            }
            
            return processedCount;
        }

        #endregion

        #region Batch Insert Benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("BatchInsert")]
        public Task<int> Baseline_IndividualInserts()
        {
            var insertedCount = 0;
            var testItems = _testData.Take(50).Select(x => new TestDataModel
            {
                Id = x.Id + 10000,
                Name = $"Baseline {x.Name}",
                Value = x.Value
            }).ToList();
            
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            
            foreach (var item in testItems)
            {
                using var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO BenchmarkData (Id, Name, Value) VALUES (@Id, @Name, @Value)";
                
                var idParam = command.CreateParameter();
                idParam.ParameterName = "@Id";
                idParam.Value = item.Id;
                command.Parameters.Add(idParam);
                
                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = item.Name;
                command.Parameters.Add(nameParam);
                
                var valueParam = command.CreateParameter();
                valueParam.ParameterName = "@Value";
                valueParam.Value = item.Value;
                command.Parameters.Add(valueParam);
                
                insertedCount += command.ExecuteNonQuery();
            }
            
            return Task.FromResult(insertedCount);
        }

        [Benchmark]
        [BenchmarkCategory("BatchInsert")]
        public Task<int> Syrx_IndividualInserts()
        {
            var insertedCount = 0;
            var testItems = _testData.Take(50).Select(x => new TestDataModel
            {
                Id = x.Id + 20000,
                Name = $"Syrx {x.Name}",
                Value = x.Value
            }).ToList();
            
            var commandSetting = new CommandSetting { 
                ConnectionAlias = "benchmark", 
                CommandText = "INSERT INTO BenchmarkData (Id, Name, Value) VALUES (@Id, @Name, @Value)" 
            };
            
            using var connection = _syrxConnector.CreateConnection(commandSetting);
            connection.Open();
            
            foreach (var item in testItems)
            {
                using var command = connection.CreateCommand();
                command.CommandText = commandSetting.CommandText;
                
                var idParam = command.CreateParameter();
                idParam.ParameterName = "@Id";
                idParam.Value = item.Id;
                command.Parameters.Add(idParam);
                
                var nameParam = command.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = item.Name;
                command.Parameters.Add(nameParam);
                
                var valueParam = command.CreateParameter();
                valueParam.ParameterName = "@Value";
                valueParam.Value = item.Value;
                command.Parameters.Add(valueParam);
                
                insertedCount += command.ExecuteNonQuery();
            }
            
            return Task.FromResult(insertedCount);
        }

        #endregion

        #region Mixed Workload Benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Mixed")]
        public Task<int> Baseline_MixedWorkload()
        {
            var totalOperations = 0;
            
            // Connection-heavy operations
            for (int i = 0; i < 5; i++)
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                
                // Query operation
                using var queryCommand = connection.CreateCommand();
                queryCommand.CommandText = "SELECT COUNT(*) FROM BenchmarkData WHERE Value > @value";
                var valueParam = queryCommand.CreateParameter();
                valueParam.ParameterName = "@value";
                valueParam.Value = i * 100;
                queryCommand.Parameters.Add(valueParam);
                queryCommand.ExecuteScalar();
                totalOperations++;
                
                // Insert operation
                using var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "INSERT INTO BenchmarkData (Id, Name, Value) VALUES (@Id, @Name, @Value)";
                
                var idParam = insertCommand.CreateParameter();
                idParam.ParameterName = "@Id";
                idParam.Value = 30000 + i;
                insertCommand.Parameters.Add(idParam);
                
                var nameParam = insertCommand.CreateParameter();
                nameParam.ParameterName = "@Name";
                nameParam.Value = $"Mixed Baseline {i}";
                insertCommand.Parameters.Add(nameParam);
                
                var insertValueParam = insertCommand.CreateParameter();
                insertValueParam.ParameterName = "@Value";
                insertValueParam.Value = i * 50;
                insertCommand.Parameters.Add(insertValueParam);
                
                insertCommand.ExecuteNonQuery();
                totalOperations++;
            }
            
            return Task.FromResult(totalOperations);
        }

        [Benchmark]
        [BenchmarkCategory("Mixed")]
        public Task<int> Syrx_MixedWorkload()
        {
            var totalOperations = 0;
            
            for (int i = 0; i < 5; i++)
            {
                // Query operation
                var queryCommandSetting = new CommandSetting { 
                    ConnectionAlias = "benchmark", 
                    CommandText = "SELECT COUNT(*) FROM BenchmarkData WHERE Value > @value" 
                };
                
                using (var connection = _syrxConnector.CreateConnection(queryCommandSetting))
                {
                    connection.Open();
                    using var queryCommand = connection.CreateCommand();
                    queryCommand.CommandText = queryCommandSetting.CommandText;
                    var valueParam = queryCommand.CreateParameter();
                    valueParam.ParameterName = "@value";
                    valueParam.Value = i * 100;
                    queryCommand.Parameters.Add(valueParam);
                    queryCommand.ExecuteScalar();
                    totalOperations++;
                }
                
                // Insert operation
                var insertCommandSetting = new CommandSetting { 
                    ConnectionAlias = "benchmark", 
                    CommandText = "INSERT INTO BenchmarkData (Id, Name, Value) VALUES (@Id, @Name, @Value)" 
                };
                
                using (var connection = _syrxConnector.CreateConnection(insertCommandSetting))
                {
                    connection.Open();
                    using var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = insertCommandSetting.CommandText;
                    
                    var idParam = insertCommand.CreateParameter();
                    idParam.ParameterName = "@Id";
                    idParam.Value = 40000 + i;
                    insertCommand.Parameters.Add(idParam);
                    
                    var nameParam = insertCommand.CreateParameter();
                    nameParam.ParameterName = "@Name";
                    nameParam.Value = $"Mixed Syrx {i}";
                    insertCommand.Parameters.Add(nameParam);
                    
                    var insertValueParam = insertCommand.CreateParameter();
                    insertValueParam.ParameterName = "@Value";
                    insertValueParam.Value = i * 50;
                    insertCommand.Parameters.Add(insertValueParam);
                    
                    insertCommand.ExecuteNonQuery();
                    totalOperations++;
                }
            }
            
            return Task.FromResult(totalOperations);
        }

        #endregion

        #region Concurrent Load Benchmarks

        [Benchmark(Baseline = true)]
        [BenchmarkCategory("Concurrent")]
        public async Task<int> Baseline_ConcurrentOperations()
        {
            const int concurrency = 5;
            
            var tasks = Enumerable.Range(0, concurrency).Select(taskId => Task.Run(() =>
            {
                var operations = 0;
                
                for (int i = 0; i < 3; i++)
                {
                    using var connection = new SqlConnection(_connectionString);
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT TOP 1 Id FROM BenchmarkData WHERE Id > @id";
                    
                    var param = command.CreateParameter();
                    param.ParameterName = "@id";
                    param.Value = (taskId * 100) + i;
                    command.Parameters.Add(param);
                    
                    command.ExecuteScalar();
                    operations++;
                }
                
                return operations;
            }));
            
            var results = await Task.WhenAll(tasks);
            return results.Sum();
        }

        [Benchmark]
        [BenchmarkCategory("Concurrent")]
        public async Task<int> Syrx_ConcurrentOperations()
        {
            const int concurrency = 5;
            
            var tasks = Enumerable.Range(0, concurrency).Select(taskId => Task.Run(() =>
            {
                var operations = 0;
                var commandSetting = new CommandSetting { 
                    ConnectionAlias = "benchmark", 
                    CommandText = "SELECT TOP 1 Id FROM BenchmarkData WHERE Id > @id" 
                };
                
                for (int i = 0; i < 3; i++)
                {
                    using var connection = _syrxConnector.CreateConnection(commandSetting);
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = commandSetting.CommandText;
                    
                    var param = command.CreateParameter();
                    param.ParameterName = "@id";
                    param.Value = (taskId * 100) + i;
                    command.Parameters.Add(param);
                    
                    command.ExecuteScalar();
                    operations++;
                }
                
                return operations;
            }));
            
            var results = await Task.WhenAll(tasks);
            return results.Sum();
        }

        #endregion

        private async Task SetupTestDatabase()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Create test table
            var createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BenchmarkData]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[BenchmarkData](
                        [Id] [int] NOT NULL PRIMARY KEY,
                        [Name] [nvarchar](100) NULL,
                        [Value] [int] NULL
                    )
                END";

            using var createCommand = new SqlCommand(createTableSql, connection);
            await createCommand.ExecuteNonQueryAsync();

            // Clear existing data
            var clearSql = "DELETE FROM BenchmarkData";
            using var clearCommand = new SqlCommand(clearSql, connection);
            await clearCommand.ExecuteNonQueryAsync();
        }

        private async Task SetupTestData()
        {
            _testData = Enumerable.Range(1, 1000).Select(i => new TestDataModel
            {
                Id = i,
                Name = $"Benchmark Item {i}",
                Value = i * 5
            }).ToList();

            // Insert initial test data
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            const int batchSize = 100;
            for (int i = 0; i < _testData.Count; i += batchSize)
            {
                var batch = _testData.Skip(i).Take(batchSize);
                var sql = "INSERT INTO BenchmarkData (Id, Name, Value) VALUES (@Id, @Name, @Value)";

                foreach (var item in batch)
                {
                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Value", item.Value);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }

    /// <summary>
    /// Custom benchmark configuration for performance testing
    /// </summary>
    public class PerformanceBenchmarkConfig : ManualConfig
    {
        public PerformanceBenchmarkConfig()
        {
            AddLogger(ConsoleLogger.Default);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Median);
            AddColumn(StatisticColumn.Min);
            AddColumn(StatisticColumn.Max);
            AddColumn(BaselineRatioColumn.RatioMean);
            AddColumn(RankColumn.Arabic);
        }
    }

    /// <summary>
    /// Performance benchmark runner and results analyzer
    /// </summary>
    public class Phase3BenchmarkRunner
    {
        public static Task<BenchmarkRunInfo[]> RunAllBenchmarks()
        {
            var config = new PerformanceBenchmarkConfig();
            
            Console.WriteLine("Starting Phase 3 Performance Benchmarks...");
            Console.WriteLine("This will compare Syrx SqlServer connector against baseline ADO.NET performance.");
            Console.WriteLine();

            var summary = BenchmarkRunner.Run<Phase3PerformanceBenchmarks>(config);
            
            Console.WriteLine();
            Console.WriteLine("=== SYRX SQLSERVER PERFORMANCE ANALYSIS ===");
            
            AnalyzeBenchmarkResults(summary);
            
            return Task.FromResult(new[] { new BenchmarkRunInfo { Summary = summary } });
        }

        private static void AnalyzeBenchmarkResults(BenchmarkDotNet.Reports.Summary summary)
        {
            var reports = summary.Reports.GroupBy(r => r.BenchmarkCase.Descriptor.Categories.FirstOrDefault() ?? "Uncategorized");
            
            foreach (var categoryGroup in reports)
            {
                Console.WriteLine($"\n--- {categoryGroup.Key} Category ---");
                
                var baseline = categoryGroup.FirstOrDefault(r => r.BenchmarkCase.Descriptor.Baseline);
                var optimized = categoryGroup.FirstOrDefault(r => !r.BenchmarkCase.Descriptor.Baseline);
                
                if (baseline != null && optimized != null)
                {
                    var baselineMean = baseline.ResultStatistics?.Mean ?? 0;
                    var optimizedMean = optimized.ResultStatistics?.Mean ?? 0;
                    
                    if (baselineMean > 0)
                    {
                        var improvement = ((baselineMean - optimizedMean) / baselineMean) * 100;
                        var speedup = baselineMean / optimizedMean;
                        
                        Console.WriteLine($"  Performance Improvement: {improvement:F1}%");
                        Console.WriteLine($"  Speed Multiplier: {speedup:F2}x");
                        
                        var baselineMemory = baseline.GcStats.GetBytesAllocatedPerOperation(baseline.BenchmarkCase);
                        var optimizedMemory = optimized.GcStats.GetBytesAllocatedPerOperation(optimized.BenchmarkCase);
                        
                        if (baselineMemory > 0 && optimizedMemory >= 0)
                        {
                            var memoryReduction = ((baselineMemory - optimizedMemory) / (double)baselineMemory) * 100;
                            Console.WriteLine($"  Memory Reduction: {memoryReduction:F1}%");
                        }
                    }
                }
            }
            
            Console.WriteLine("\n=== SYRX SQL SERVER CONNECTOR PERFORMANCE SUMMARY ===");
            Console.WriteLine("Performance Comparison Areas:");
            Console.WriteLine("• Connection Creation: Syrx connector vs raw ADO.NET");
            Console.WriteLine("• Repeated Queries: Command preparation and execution");
            Console.WriteLine("• Large Result Sets: Data reader performance");
            Console.WriteLine("• Batch Operations: Individual insert performance");
            Console.WriteLine("• Mixed Workloads: Query and command operations");
            Console.WriteLine("• Concurrent Access: Multi-threaded operation handling");
        }
    }

    public class BenchmarkRunInfo
    {
        public BenchmarkDotNet.Reports.Summary Summary { get; set; } = null!;
    }

    public class TestDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
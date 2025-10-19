using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using Syrx.SqlServer.Tests.Performance.Repositories;

namespace Syrx.SqlServer.Tests.Performance.Benchmarks
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 3)]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MarkdownExporter, HtmlExporter]
    public class BulkOperationsBenchmarks
    {
        private PerformanceTestRepository _repository = null!;
        private IServiceProvider _serviceProvider = null!;
        private PerformanceTestFixture _fixture = null!;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _fixture = new PerformanceTestFixture();
            await _fixture.InitializeAsync();

            _serviceProvider = PerformanceTestHelper.CreateServiceProvider(_fixture.ConnectionString);
            _repository = _serviceProvider.GetRequiredService<PerformanceTestRepository>();
        }

        [GlobalCleanup]
        public async Task GlobalCleanup()
        {
            await _fixture.DisposeAsync();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Params(100, 500, 1000, 2500)]
        public int BatchSize { get; set; }

        [Benchmark]
        public async Task<int> BulkInsert_VariableBatchSize()
        {
            var results = await _repository.BulkInsertAsync(BatchSize, 1);
            return results.Sum(r => r.InsertedCount);
        }

        [Benchmark]
        public async Task<int> BatchUpdate_Electronics()
        {
            var results = await _repository.BatchUpdateAsync(1, 1.05m);
            return results.Sum(r => r.UpdatedCount);
        }

        [Benchmark]
        public async Task<int> GetPaginated_Page1()
        {
            var results = await _repository.GetPaginatedAsync(100, 1);
            return results.Count();
        }

        [Benchmark]
        public async Task<int> GetPaginated_Page10()
        {
            var results = await _repository.GetPaginatedAsync(100, 10);
            return results.Count();
        }

        [Benchmark]
        public async Task<int> GetCategoryStats_Aggregated()
        {
            var results = await _repository.GetCategoryStatsAsync();
            return results.Count();
        }

        [Benchmark]
        public async Task<int> GetDailyStats_Last30Days()
        {
            var results = await _repository.GetDailyStatsAsync(30);
            return results.Count();
        }
    }
}
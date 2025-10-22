using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using Syrx.SqlServer.Tests.Performance.Models;
using Syrx.SqlServer.Tests.Performance.Repositories;

namespace Syrx.SqlServer.Tests.Performance.Benchmarks
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MarkdownExporter, HtmlExporter]
    public class BasicOperationsBenchmarks
    {
        private PerformanceTestRepository _repository = null!;
        private CategoryRepository _categoryRepository = null!;
        private SimpleKeyValueRepository _kvRepository = null!;
        private IServiceProvider _serviceProvider = null!;
        private PerformanceTestFixture _fixture = null!;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _fixture = new PerformanceTestFixture();
            await _fixture.InitializeAsync();

            _serviceProvider = PerformanceTestHelper.CreateServiceProvider(_fixture.ConnectionString);
            _repository = _serviceProvider.GetRequiredService<PerformanceTestRepository>();
            _categoryRepository = _serviceProvider.GetRequiredService<CategoryRepository>();
            _kvRepository = _serviceProvider.GetRequiredService<SimpleKeyValueRepository>();
        }

        [GlobalCleanup]
        public async Task GlobalCleanup()
        {
            await _fixture.DisposeAsync();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Benchmark]
        public async Task<int> GetAll_First100()
        {
            var results = await _repository.GetAllAsync();
            return results.Take(100).Count();
        }

        [Benchmark]
        public async Task<PerformanceTestEntity?> GetById_SingleRecord()
        {
            return await _repository.GetByIdAsync(1);
        }

        [Benchmark]
        public async Task<int> GetByCategory_Electronics()
        {
            var results = await _repository.GetByCategoryAsync(1, 50);
            return results.Count();
        }

        [Benchmark]
        public async Task<bool> Insert_SingleRecord()
        {
            var entity = new PerformanceTestEntity
            {
                Name = $"Benchmark Test {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}",
                Description = "Performance benchmark test record",
                Value = 123.45m,
                CategoryId = 1,
                IsActive = true,
                JsonData = """{"benchmark": true, "timestamp": "2025-10-12T10:00:00Z"}"""
            };
            
            return await _repository.InsertAsync(entity);
        }

        [Benchmark]
        public async Task<SimpleKeyValue?> KeyValue_Get()
        {
            return await _kvRepository.GetAsync("config.timeout");
        }

        [Benchmark]
        public async Task<bool> KeyValue_Set()
        {
            return await _kvRepository.SetAsync($"benchmark.{DateTime.UtcNow.Ticks}", "test_value");
        }

        [Benchmark]
        public async Task<int> Categories_GetAll()
        {
            var results = await _categoryRepository.GetAllAsync();
            return results.Count();
        }

        [Benchmark]
        public async Task<int> Search_ByName()
        {
            var results = await _repository.SearchByNameAsync("Item", 20);
            return results.Count();
        }

        [Benchmark]
        public async Task<int> GetRecent_Last7Days()
        {
            var results = await _repository.GetRecentAsync(7, 100);
            return results.Count();
        }

        [Benchmark]
        public async Task<int> GetWithCategories_Joined()
        {
            var results = await _repository.GetWithCategoriesAsync(50);
            return results.Count();
        }
    }
}
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.Extensions.DependencyInjection;
using Syrx.SqlServer.Tests.Performance.Repositories;

namespace Syrx.SqlServer.Tests.Performance.Benchmarks
{
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MeanColumn, MedianColumn]
    [MarkdownExporter, HtmlExporter]
    [ThreadingDiagnoser]
    public class ConcurrencyBenchmarks
    {
        private ConcurrentTestRepository _repository = null!;
        private IServiceProvider _serviceProvider = null!;
        private PerformanceTestFixture _fixture = null!;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _fixture = new PerformanceTestFixture();
            await _fixture.InitializeAsync();

            _serviceProvider = PerformanceTestHelper.CreateServiceProvider(_fixture.ConnectionString);
            _repository = _serviceProvider.GetRequiredService<ConcurrentTestRepository>();
            
            // Reset counters before testing
            await _repository.ResetCountersAsync();
        }

        [GlobalCleanup]
        public async Task GlobalCleanup()
        {
            await _fixture.DisposeAsync();
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        [Params(1, 2, 4, 8)]
        public int ThreadCount { get; set; }

        [Benchmark]
        public async Task ConcurrentIncrements_MultipleThreads()
        {
            var tasks = new List<Task>();
            
            for (int i = 0; i < ThreadCount; i++)
            {
                int threadId = i + 1;
                tasks.Add(Task.Run(async () =>
                {
                    for (int j = 0; j < 50; j++)
                    {
                        await _repository.IncrementCounterAsync(threadId);
                        await Task.Delay(1); // Small delay to simulate real workload
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
        }

        [Benchmark]
        public async Task ConcurrentReads_SameData()
        {
            var tasks = new List<Task<int>>();
            
            for (int i = 0; i < ThreadCount * 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var results = await _repository.GetAllCountersAsync();
                    return results.Count();
                }));
            }
            
            var results = await Task.WhenAll(tasks);
            // Return sum to ensure all tasks complete
            var _ = results.Sum();
        }

        [Benchmark]
        public async Task<int> ReadAfterWrite_Consistency()
        {
            var threadId = Random.Shared.Next(1000, 9999);
            
            // Write
            await _repository.IncrementCounterAsync(threadId);
            
            // Immediate read
            var results = await _repository.GetAllCountersAsync();
            return results.Count(r => r.ThreadId == threadId);
        }
    }
}
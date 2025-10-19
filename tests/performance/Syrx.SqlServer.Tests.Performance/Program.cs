using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using Syrx.SqlServer.Tests.Performance.Benchmarks;

namespace Syrx.SqlServer.Tests.Performance
{
    /// <summary>
    /// Main program for running Syrx.SqlServer performance benchmarks
    /// 
    /// Usage:
    /// dotnet run --configuration Release
    /// dotnet run --configuration Release -- --filter "*BasicOperations*"
    /// dotnet run --configuration Release -- --filter "*BulkOperations*"
    /// dotnet run --configuration Release -- --filter "*Concurrency*"
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Syrx.SqlServer Performance Tests");
            Console.WriteLine("================================");
            Console.WriteLine();
            
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .AddJob(Job.Default
                    .WithRuntime(CoreRuntime.Core80)
                    .WithPlatform(Platform.X64)
                    .WithJit(Jit.RyuJit))
                .AddExporter(HtmlExporter.Default)
                .AddExporter(MarkdownExporter.GitHub)
                .AddExporter(MarkdownExporter.StackOverflow)
                .AddLogger(ConsoleLogger.Default);

            if (args.Length > 0 && args.Contains("--help"))
            {
                ShowHelp();
                return;
            }

            try
            {
                // Determine which benchmarks to run based on arguments
                var benchmarksToRun = GetBenchmarksToRun(args);
                
                Console.WriteLine($"Running {benchmarksToRun.Count} benchmark suite(s)...");
                Console.WriteLine();

                var summaries = new List<BenchmarkDotNet.Reports.Summary>();
                
                foreach (var benchmarkType in benchmarksToRun)
                {
                    Console.WriteLine($"Starting benchmark: {benchmarkType.Name}");
                    var summary = BenchmarkRunner.Run(benchmarkType, config);
                    summaries.Add(summary);
                    Console.WriteLine($"Completed benchmark: {benchmarkType.Name}");
                    Console.WriteLine();
                }

                // Print summary
                Console.WriteLine("Performance Test Results Summary");
                Console.WriteLine("================================");
                
                foreach (var summary in summaries)
                {
                    Console.WriteLine($"{summary.Title}:");
                    Console.WriteLine($"  - Total benchmarks: {summary.Reports.Length}");
                    Console.WriteLine($"  - Successful: {summary.Reports.Count(r => r.Success)}");
                    Console.WriteLine($"  - Failed: {summary.Reports.Count(r => !r.Success)}");
                    
                    if (summary.HasCriticalValidationErrors)
                    {
                        Console.WriteLine($"  - Critical validation errors detected");
                    }
                    
                    Console.WriteLine();
                }

                Console.WriteLine("Performance tests completed successfully!");
                Console.WriteLine("Check the generated HTML and Markdown reports for detailed results.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running performance tests: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        private static List<Type> GetBenchmarksToRun(string[] args)
        {
            var allBenchmarks = new List<Type>
            {
                typeof(BasicOperationsBenchmarks),
                typeof(BulkOperationsBenchmarks),
                typeof(ConcurrencyBenchmarks),
                typeof(CommandFlagSettingsBenchmarks)
            };

            // Check for filter argument
            var filterArg = Array.Find(args, arg => arg.StartsWith("--filter"));
            if (filterArg != null)
            {
                var filterValue = filterArg.Split('=').LastOrDefault()?.Trim('"', '\'');
                if (!string.IsNullOrEmpty(filterValue))
                {
                    return allBenchmarks.Where(t => 
                        t.Name.Contains(filterValue.Replace("*", ""), StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            // Check for specific benchmark arguments
            var selectedBenchmarks = new List<Type>();
            
            if (args.Contains("--basic") || args.Contains("-b"))
                selectedBenchmarks.Add(typeof(BasicOperationsBenchmarks));
                
            if (args.Contains("--bulk") || args.Contains("-k"))
                selectedBenchmarks.Add(typeof(BulkOperationsBenchmarks));
                
            if (args.Contains("--concurrency") || args.Contains("-c"))
                selectedBenchmarks.Add(typeof(ConcurrencyBenchmarks));
                
            if (args.Contains("--flags") || args.Contains("-f"))
                selectedBenchmarks.Add(typeof(CommandFlagSettingsBenchmarks));

            // If no specific benchmarks selected, run all
            return selectedBenchmarks.Any() ? selectedBenchmarks : allBenchmarks;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Syrx.SqlServer Performance Tests");
            Console.WriteLine("Usage: dotnet run --configuration Release [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help              Show this help message");
            Console.WriteLine("  --basic, -b         Run basic operations benchmarks only");
            Console.WriteLine("  --bulk, -k          Run bulk operations benchmarks only");
            Console.WriteLine("  --concurrency, -c   Run concurrency benchmarks only");
            Console.WriteLine("  --flags, -f         Run CommandFlagSettings benchmarks only");
            Console.WriteLine("  --filter=<pattern>  Run benchmarks matching the pattern (supports wildcards)");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run --configuration Release");
            Console.WriteLine("  dotnet run --configuration Release --basic");
            Console.WriteLine("  dotnet run --configuration Release --flags");
            Console.WriteLine("  dotnet run --configuration Release --filter=\"*Basic*\"");
            Console.WriteLine("  dotnet run --configuration Release --bulk --concurrency");
            Console.WriteLine();
            Console.WriteLine("The tests will automatically:");
            Console.WriteLine("  1. Start a SQL Server container using Testcontainers");
            Console.WriteLine("  2. Initialize the performance database schema");
            Console.WriteLine("  3. Seed test data (10,000+ records)");
            Console.WriteLine("  4. Run the selected performance benchmarks");
            Console.WriteLine("  5. Generate HTML and Markdown reports");
            Console.WriteLine("  6. Clean up the container when finished");
            Console.WriteLine();
            Console.WriteLine("Results will be saved to:");
            Console.WriteLine("  - BenchmarkDotNet.Artifacts/results/ (detailed reports)");
            Console.WriteLine("  - Console output (summary)");
        }
    }
}
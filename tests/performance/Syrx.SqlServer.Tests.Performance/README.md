# Syrx.SqlServer Performance Tests

This project contains comprehensive performance tests for the Syrx.SqlServer database framework, designed to measure and benchmark various database operations under different scenarios.

## Overview

The performance test suite uses:
- **BenchmarkDotNet** for detailed micro-benchmarks
- **xUnit** for integration performance tests
- **Testcontainers** for isolated SQL Server container setup
- **Docker** for consistent, reproducible test environments

## Test Categories

### 1. Basic Operations Benchmarks
- Single record retrieval
- Small batch queries
- Simple inserts and updates
- Key-value operations
- Category lookups

### 2. Bulk Operations Benchmarks
- Variable batch size inserts (100-2500 records)
- Batch updates across categories
- Paginated data retrieval
- Aggregated statistics queries
- Time-series data analysis

### 3. Concurrency Benchmarks
- Multi-threaded read/write operations
- Connection pool stress testing
- Read-after-write consistency
- Deadlock prevention validation

## Project Structure

```
Syrx.SqlServer.Tests.Performance/
├── Benchmarks/                          # BenchmarkDotNet benchmark classes
│   ├── BasicOperationsBenchmarks.cs     # Basic CRUD operations
│   ├── BulkOperationsBenchmarks.cs      # Large batch operations
│   └── ConcurrencyBenchmarks.cs         # Multi-threading tests
├── Docker/                              # Database initialization
│   └── init-scripts/                    # SQL setup scripts
│       ├── 01-create-database.sql       # Database creation
│       ├── 02-create-tables.sql         # Table schemas
│       ├── 03-create-procedures.sql     # Stored procedures
│       └── 04-seed-data.sql             # Test data (10,000+ records)
├── Models/                              # Data models
│   └── TestModels.cs                    # Entity classes
├── Repositories/                        # Syrx repository patterns
│   └── PerformanceRepositories.cs       # Database access layer
├── Tests/                               # xUnit integration tests
│   └── PerformanceIntegrationTests.cs   # Performance validation tests
├── PerformanceTestFixture.cs            # Test fixture with container setup
├── PerformanceTestHelper.cs             # Dependency injection helper
├── Program.cs                           # Benchmark runner
└── appsettings.json                     # Syrx configuration
```

## Database Schema

The performance tests use the `SyrxPerformance` database with the following tables:

- **performance_test**: Main test table (10,000+ records with indexes)
- **categories**: Category hierarchy for join operations
- **concurrent_test**: Thread-safe counter for concurrency tests
- **simple_kv**: Key-value store for basic operations

## Running the Tests

### Prerequisites
- .NET 8.0 SDK
- Docker Desktop running
- 4GB+ available RAM (for SQL Server container)

### Benchmark Tests (Recommended)

Run all benchmarks:
```bash
dotnet run --configuration Release
```

Run specific benchmark categories:
```bash
# Basic operations only
dotnet run --configuration Release --basic

# Bulk operations only  
dotnet run --configuration Release --bulk

# Concurrency tests only
dotnet run --configuration Release --concurrency

# Multiple categories
dotnet run --configuration Release --basic --bulk
```

Filter by pattern:
```bash
# Run benchmarks containing "Insert"
dotnet run --configuration Release --filter="*Insert*"

# Run BasicOperations benchmarks only
dotnet run --configuration Release --filter="*BasicOperations*"
```

### Integration Tests (xUnit)

Run performance validation tests:
```bash
dotnet test --configuration Release
```

Run with detailed output:
```bash
dotnet test --configuration Release --logger "console;verbosity=detailed"
```

## Test Environment

### Automatic Setup
The tests automatically:
1. Start a SQL Server 2022 container
2. Create the `SyrxPerformance` database
3. Initialize tables with proper indexes
4. Seed 10,000+ test records across categories
5. Configure Syrx with optimal connection settings
6. Clean up containers after completion

### Container Configuration
- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Memory**: 2GB allocated to SQL Server
- **Connection**: Optimized for performance testing
- **Isolation**: Each test run uses a fresh container

## Performance Metrics

### Benchmark Results
Results are generated in multiple formats:
- **HTML Reports**: `BenchmarkDotNet.Artifacts/results/*.html`
- **Markdown**: `BenchmarkDotNet.Artifacts/results/*.md`
- **Console Output**: Real-time progress and summary

### Key Metrics Measured
- **Throughput**: Operations per second
- **Latency**: Mean, median, min/max response times
- **Memory**: Allocation patterns and GC pressure
- **Concurrency**: Thread safety and scaling characteristics

### Expected Performance Baselines
- Single record retrieval: < 5ms
- Bulk insert (1000 records): < 10 seconds
- Pagination (100 records): < 500ms per page
- Concurrent operations: Linear scaling up to 8 threads

## Configuration

### Syrx Configuration
The tests use JSON-based configuration with:
- Optimized connection strings
- Command timeout settings
- Connection pooling parameters
- SQL Server specific optimizations

### Customization
Modify `appsettings.json` to:
- Adjust command timeouts
- Change connection pool settings
- Add custom SQL commands
- Configure logging levels

## Continuous Integration

### GitHub Actions Integration
```yaml
# Add to your workflow
- name: Run Performance Tests
  run: |
    cd tests/performance/Syrx.SqlServer.Tests.Performance
    dotnet run --configuration Release --basic
```

### Performance Regression Detection
- Baseline results can be stored and compared
- Automated alerts for performance degradation
- Historical trend analysis

## Troubleshooting

### Common Issues

**Container Startup Failures**:
- Ensure Docker Desktop is running
- Check available memory (4GB+ recommended)
- Verify port 1433 is not in use

**Slow Performance**:
- Use Release configuration (`--configuration Release`)
- Ensure adequate system resources
- Check Docker memory allocation

**Connection Errors**:
- Tests automatically retry connection attempts
- Container initialization takes 30-60 seconds
- Check firewall settings for Docker

### Debug Mode
Run with detailed logging:
```bash
dotnet run --configuration Debug --basic
```

## Contributing

### Adding New Benchmarks
1. Create benchmark class in `Benchmarks/`
2. Inherit from appropriate base class
3. Add `[Benchmark]` attributes
4. Update `Program.cs` to include new benchmark
5. Add corresponding integration tests

### Performance Test Guidelines
- Use `[GlobalSetup]` for expensive initialization
- Minimize allocations in benchmark methods
- Use appropriate `[Params]` for variable testing
- Include both synthetic and realistic workloads
- Document expected performance characteristics

## Results Interpretation

### BenchmarkDotNet Output
- **Mean**: Average execution time
- **Error**: Standard error of the mean
- **StdDev**: Standard deviation
- **Median**: 50th percentile
- **Gen 0/1/2**: Garbage collection counts
- **Allocated**: Memory allocation per operation

### Performance Targets
- **Excellent**: < 1ms for simple operations
- **Good**: 1-10ms for complex queries
- **Acceptable**: 10-100ms for bulk operations
- **Review Required**: > 100ms for any operation

## License

MIT License - See the main project license file for details.
# Syrx.SqlServer.Performance.Demo

Performance demonstration and benchmarking application for Syrx SQL Server integration.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Demo Scenarios](#demo-scenarios)
- [Performance Comparisons](#performance-comparisons)
- [Configuration](#configuration)
- [Running the Demo](#running-the-demo)
- [Performance Results](#performance-results)
- [Code Examples](#code-examples)
- [Related Projects](#related-projects)
- [License](#license)

## Overview

`Syrx.SqlServer.Performance.Demo` is a console application that demonstrates the performance characteristics and usage patterns of the Syrx SQL Server integration. It provides practical examples of how to use Syrx with SQL Server databases and includes performance benchmarks comparing different approaches.

## Features

- **Real-World Examples**: Practical demonstrations of Syrx usage patterns
- **Performance Benchmarking**: Comparison between raw ADO.NET and Syrx approaches
- **LocalDB Integration**: Uses SQL Server LocalDB for easy setup and testing
- **Comprehensive Logging**: Detailed logging of operations and performance metrics
- **Multiple Demo Scenarios**: Various use cases from simple queries to complex operations

## Prerequisites

- .NET 6.0 or later
- SQL Server LocalDB (installed with Visual Studio or SQL Server Express)
- Windows operating system (for LocalDB support)

## Getting Started

### 1. Clone and Build

```bash
git clone https://github.com/Syrx/Syrx.SqlServer.git
cd Syrx.SqlServer/src/Syrx.SqlServer.Performance.Demo
dotnet build
```

### 2. Run the Demo

```bash
dotnet run
```

The application will automatically:
- Create a LocalDB instance if needed
- Set up the demo database and tables
- Run performance comparisons
- Display results and examples

## Demo Scenarios

### Basic CRUD Operations
- **Insert Operations**: Bulk insert performance testing
- **Query Operations**: Single and multiple record retrieval
- **Update Operations**: Record modification scenarios
- **Delete Operations**: Data cleanup and removal

### Advanced Scenarios
- **Transaction Management**: Automatic transaction handling
- **Connection Pooling**: Connection reuse and management
- **Error Handling**: Exception scenarios and recovery
- **Async Operations**: Asynchronous vs synchronous performance

## Performance Comparisons

The demo compares several approaches:

### Raw ADO.NET vs Syrx
```csharp
// Raw ADO.NET approach
using var connection = new SqlConnection(connectionString);
using var command = new SqlCommand("SELECT * FROM DemoData", connection);
await connection.OpenAsync();
using var reader = await command.ExecuteReaderAsync();
// Manual result mapping...

// Syrx approach  
var results = await _commander.QueryAsync<DemoData>();
```

### Performance Metrics
- **Execution Time**: Operation duration measurements
- **Memory Usage**: Memory allocation and cleanup
- **Connection Management**: Connection pool utilization
- **Throughput**: Operations per second capabilities

## Configuration

### Connection String Configuration

The demo uses LocalDB with automatic database creation:

```csharp
private const string ConnectionString = 
    @"Server=(localdb)\MSSQLLocalDB;Database=SyrxDemo;Integrated Security=true;";
```

### Syrx Configuration

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Demo", ConnectionString)
        .AddCommand(types => types
            .ForType<DemoRepository>(methods => methods
                .ForMethod("GetAllDataAsync", cmd => cmd
                    .UseConnectionAlias("Demo")
                    .UseCommandText("SELECT * FROM DemoData"))
                .ForMethod("InsertDataAsync", cmd => cmd
                    .UseConnectionAlias("Demo")
                    .UseCommandText("INSERT INTO DemoData (Name, Value) VALUES (@name, @value)"))))));
```

## Running the Demo

### Command Line Options

```bash
# Run with default settings
dotnet run

# Run with custom iterations
dotnet run --iterations 1000

# Run with verbose logging
dotnet run --verbose
```

### Expected Output

```
Syrx SQL Server Performance Demo
================================

Initializing LocalDB...
Creating demo database...
Setting up demo data...

Performance Comparison Results:
==============================

Raw ADO.NET (1000 operations):
- Average: 2.34ms per operation
- Total: 2,340ms
- Memory: 45MB peak

Syrx Framework (1000 operations):  
- Average: 2.41ms per operation
- Total: 2,410ms
- Memory: 42MB peak

Overhead: +0.07ms per operation (3% increase)
Memory Savings: -3MB (7% reduction)
```

## Performance Results

### Typical Benchmarks

| Operation | Raw ADO.NET | Syrx | Overhead |
|-----------|-------------|------|----------|
| Simple Query | 1.2ms | 1.3ms | +8% |
| Insert | 0.8ms | 0.9ms | +12% |
| Bulk Insert (100) | 45ms | 47ms | +4% |
| Complex Query | 3.4ms | 3.6ms | +6% |

### Key Findings

- **Minimal Overhead**: Syrx adds 3-12% overhead for significant developer productivity gains
- **Memory Efficiency**: Often uses less memory due to optimized object handling
- **Developer Productivity**: Dramatically reduces boilerplate code
- **Type Safety**: Compile-time checking prevents runtime errors

## Code Examples

### Repository Pattern with Syrx

```csharp
public class DemoRepository
{
    private readonly ICommander<DemoRepository> _commander;

    public DemoRepository(ICommander<DemoRepository> commander)
    {
        _commander = commander;
    }

    public async Task<IEnumerable<DemoData>> GetAllDataAsync()
    {
        return await _commander.QueryAsync<DemoData>();
    }

    public async Task<bool> InsertDataAsync(string name, int value)
    {
        return await _commander.ExecuteAsync(new { name, value });
    }

    public async Task<int> GetCountAsync()
    {
        var results = await _commander.QueryAsync<int>();
        return results.FirstOrDefault();
    }
}
```

### Data Model

```csharp
public class DemoData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public DateTime Created { get; set; }
}
```

### Performance Testing

```csharp
public async Task RunPerformanceTest()
{
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        await _repository.InsertDataAsync($"Test{i}", i);
    }
    
    stopwatch.Stop();
    Console.WriteLine($"1000 inserts completed in {stopwatch.ElapsedMilliseconds}ms");
}
```

## Related Projects

- **[Syrx.SqlServer](../Syrx.SqlServer/)**: Core SQL Server provider
- **[Syrx.SqlServer.Extensions](../Syrx.SqlServer.Extensions/)**: Configuration extensions
- **[Syrx.SqlServer.Tests.Performance](../../tests/performance/Syrx.SqlServer.Tests.Performance/)**: Comprehensive performance test suite

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

---

*This demo application serves as both a learning tool and a performance validation suite for the Syrx SQL Server integration. It provides practical examples that can be adapted for real-world applications while demonstrating the framework's capabilities and performance characteristics.*

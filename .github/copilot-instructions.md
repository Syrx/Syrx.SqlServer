# Copilot Instructions for Syrx.SqlServer

This document provides comprehensive guidance for GitHub Copilot and other AI assistants working with the Syrx.SqlServer codebase.

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture Guidelines](#architecture-guidelines)
- [Code Style and Standards](#code-style-and-standards)
- [Documentation Standards](#documentation-standards)
- [Testing Patterns](#testing-patterns)
- [Performance Considerations](#performance-considerations)
- [Common Patterns](#common-patterns)
- [Repository Patterns](#repository-patterns)
- [Configuration Patterns](#configuration-patterns)
- [Error Handling](#error-handling)

## Project Overview

Syrx.SqlServer is a high-performance Microsoft SQL Server provider for the Syrx data access framework. It provides:

- **Configuration-driven data access**: SQL commands are externalized from code
- **Type-safe operations**: Strong typing throughout the execution pipeline
- **High performance**: Built on Dapper and Microsoft.Data.SqlClient
- **Dependency injection integration**: First-class DI support
- **Connection management**: Intelligent pooling and resource management

### Key Design Principles

1. **Separation of Concerns**: Clean separation between data access, business logic, and configuration
2. **Performance First**: Optimize for speed while maintaining developer productivity  
3. **Type Safety**: Compile-time verification wherever possible
4. **Configuration-Driven**: SQL commands defined externally, not embedded in code
5. **Convention over Configuration**: Sensible defaults with override capabilities

## Architecture Guidelines

### Package Structure

```
Syrx.SqlServer/
├── src/
│   ├── Syrx.SqlServer/                                    # Core package
│   ├── Syrx.SqlServer.Extensions/                         # DI extensions
│   ├── Syrx.Commanders.Databases.Connectors.SqlServer/    # Database connector
│   ├── Syrx.Commanders.Databases.Connectors.SqlServer.Extensions/ # Connector DI
│   └── Syrx.SqlServer.Performance.Demo/                   # Demo application
├── tests/
│   ├── unit/                                              # Unit tests
│   ├── integration/                                       # Integration tests
│   └── performance/                                       # Performance tests
└── .docs/                                                 # Technical documentation
```

### Layered Architecture

```
Application Layer (Repositories)
    ↓
Commander Layer (ICommander<T>)
    ↓  
Connector Layer (SqlServerDatabaseConnector)
    ↓
Database Layer (Microsoft.Data.SqlClient)
```

### Key Components

- **SqlServerDatabaseConnector**: Core database connectivity
- **ServiceCollectionExtensions**: DI registration helpers
- **SqlServerConnectorExtensions**: Configuration builder extensions
- **ICommander<T>**: Primary interface for data operations

## Code Style and Standards

### C# Coding Standards

```csharp
// Use explicit typing for clarity
SqlServerDatabaseConnector connector = new SqlServerDatabaseConnector(settings);

// Prefer async/await patterns
public async Task<User> GetUserAsync(int id)
{
    var users = await _commander.QueryAsync<User>(new { id });
    return users.FirstOrDefault();
}

// Use meaningful parameter names
public static IServiceCollection AddSqlServer(
    this IServiceCollection services, 
    ServiceLifetime lifetime = ServiceLifetime.Transient)

// Prefer expression-bodied members for simple operations
public string ConnectionString => _settings.GetConnectionString(Alias);
```

### File Organization

- **One class per file**: Each class should have its own file
- **Meaningful file names**: File name should match the primary class name
- **Namespace alignment**: Namespace should reflect folder structure
- **Using statements**: Organize using statements (System first, then third-party, then local)

### Naming Conventions

- **Classes**: PascalCase (e.g., `SqlServerDatabaseConnector`)
- **Methods**: PascalCase (e.g., `GetUserAsync`)
- **Parameters**: camelCase (e.g., `connectionString`)
- **Private fields**: _camelCase (e.g., `_commander`)
- **Constants**: PascalCase (e.g., `DefaultTimeout`)

## Documentation Standards

### XML Documentation

All public APIs must have comprehensive XML documentation:

```csharp
/// <summary>
/// Provides SQL Server database connectivity for the Syrx framework using Microsoft.Data.SqlClient.
/// </summary>
/// <remarks>
/// This connector extends the base DatabaseConnector to provide SQL Server-specific database connection functionality.
/// It uses the SqlClientFactory to create connections and inherits all the connection management capabilities
/// from the base DatabaseConnector class.
/// </remarks>
public class SqlServerDatabaseConnector : DatabaseConnector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseConnector"/> class.
    /// </summary>
    /// <param name="settings">The commander settings containing connection string and command configurations.</param>
    /// <remarks>
    /// The constructor passes the settings and SqlClientFactory.Instance to the base DatabaseConnector,
    /// enabling SQL Server database connections with the configured connection strings and commands.
    /// </remarks>
    public SqlServerDatabaseConnector(ICommanderSettings settings) 
        : base(settings, () => SqlClientFactory.Instance)
    {
    }
}
```

### README Documentation

Each project must have a comprehensive README.md with:

- **Overview**: Clear description of the package purpose
- **Installation**: Package installation instructions
- **Usage Examples**: Practical code examples
- **Configuration**: Configuration options and patterns
- **API Reference**: Links to detailed API documentation
- **Performance**: Performance characteristics and benchmarks
- **Related Packages**: Links to related packages in the ecosystem

### Technical Documentation

The `.docs` folder contains comprehensive technical documentation following the pattern established in the submodules:

- **Architecture diagrams**: ASCII art diagrams showing component relationships
- **Usage patterns**: Common usage scenarios and best practices
- **Performance benchmarks**: Concrete performance data and comparisons
- **Migration guides**: How to migrate from other technologies

## Testing Patterns

### Unit Tests

```csharp
[Fact]
public void SqlServerDatabaseConnector_Constructor_SetsFactoryCorrectly()
{
    // Arrange
    var settings = new Mock<ICommanderSettings>();
    
    // Act
    var connector = new SqlServerDatabaseConnector(settings.Object);
    
    // Assert
    Assert.NotNull(connector);
}
```

### Integration Tests

```csharp
public class SqlServerIntegrationTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;
    
    public SqlServerIntegrationTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task GetUserAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var repository = _fixture.GetService<UserRepository>();
        
        // Act  
        var user = await repository.GetUserAsync(1);
        
        // Assert
        Assert.NotNull(user);
        Assert.Equal(1, user.Id);
    }
}
```

### Performance Tests

```csharp
[Fact]
public async Task QueryPerformance_SqlServer_MeetsPerformanceTargets()
{
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        await _repository.GetUserAsync(i);
    }
    
    stopwatch.Stop();
    
    // Should complete 1000 queries in under 2 seconds
    Assert.True(stopwatch.ElapsedMilliseconds < 2000);
}
```

## Performance Considerations

### Connection Management

- **Use connection pooling**: Configure appropriate pool sizes in connection strings
- **Dispose connections properly**: The framework handles this automatically
- **Avoid long-running connections**: Create connections per operation

```csharp
// Good: Proper connection string with pooling
"Server=localhost;Database=MyApp;Integrated Security=true;Max Pool Size=100;Min Pool Size=5;"

// Good: Framework handles connection lifecycle
public async Task<User> GetUserAsync(int id)
{
    return await _commander.QueryAsync<User>(new { id }); // Connection auto-managed
}
```

### Query Optimization

- **Use parameterized queries**: All queries are parameterized by default
- **Leverage SQL Server query plan caching**: Consistent parameterized queries
- **Consider read/write separation**: Use different connections for reads and writes

```csharp
// Good: Parameterized query
.UseCommandText("SELECT * FROM Users WHERE Id = @id")

// Good: Read/write separation
.AddConnectionString("ReadDB", readOnlyConnectionString)
.AddConnectionString("WriteDB", readWriteConnectionString)
```

## Common Patterns

### Repository Pattern

```csharp
public class UserRepository
{
    private readonly ICommander<UserRepository> _commander;
    
    public UserRepository(ICommander<UserRepository> commander)
    {
        _commander = commander;
    }
    
    public async Task<User> GetByIdAsync(int id)
    {
        var users = await _commander.QueryAsync<User>(new { id });
        return users.FirstOrDefault();
    }
    
    public async Task<bool> CreateAsync(User user)
    {
        return await _commander.ExecuteAsync(user);
    }
}
```

### Command Resolution Pattern

Commands are resolved using: `{Namespace}.{ClassName}.{MethodName}`

```csharp
// For method: MyApp.Repositories.UserRepository.GetByIdAsync
// Configuration key: "MyApp.Repositories.UserRepository.GetByIdAsync"
.ForMethod(nameof(UserRepository.GetByIdAsync), command => command
    .UseConnectionAlias("Default")
    .UseCommandText("SELECT * FROM Users WHERE Id = @id"))
```

### Multi-mapping Pattern

```csharp
public async Task<IEnumerable<User>> GetUsersWithProfilesAsync()
{
    return await _commander.QueryAsync<User, Profile, User>(
        (user, profile) => 
        {
            user.Profile = profile;
            return user;
        });
}

// Configuration
.ForMethod("GetUsersWithProfilesAsync", command => command
    .UseCommandText(@"
        SELECT u.Id, u.Name, u.Email,
               p.Id, p.Bio, p.Avatar
        FROM Users u
        LEFT JOIN Profiles p ON u.Id = p.UserId")
    .SplitOn("Id"))
```

## Repository Patterns

### Standard Repository Structure

```csharp
public class ProductRepository
{
    private readonly ICommander<ProductRepository> _commander;
    
    public ProductRepository(ICommander<ProductRepository> commander)
    {
        _commander = commander ?? throw new ArgumentNullException(nameof(commander));
    }
    
    // Query operations - no transactions
    public async Task<Product> GetByIdAsync(int id)
    {
        var products = await _commander.QueryAsync<Product>(new { id });
        return products.FirstOrDefault();
    }
    
    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _commander.QueryAsync<Product>(new { category });
    }
    
    // Execute operations - automatic transactions
    public async Task<bool> CreateAsync(Product product)
    {
        return await _commander.ExecuteAsync(product);
    }
    
    public async Task<bool> UpdateAsync(Product product)
    {
        return await _commander.ExecuteAsync(product);
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        return await _commander.ExecuteAsync(new { id });
    }
}
```

### Repository Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Configure Syrx
    services.UseSyrx(builder => builder.UseSqlServer(/* configuration */));
    
    // Register repositories
    services.AddScoped<UserRepository>();
    services.AddScoped<ProductRepository>();
    services.AddScoped<OrderRepository>();
}
```

## Configuration Patterns

### Basic Configuration

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Default", connectionString)
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetUsers", cmd => cmd
                    .UseConnectionAlias("Default")
                    .UseCommandText("SELECT * FROM Users"))))));
```

### Advanced Configuration

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Primary", primaryConnection)
        .AddConnectionString("ReadOnly", readOnlyConnection)
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetUsers", cmd => cmd
                    .UseConnectionAlias("ReadOnly")
                    .UseCommandText("SELECT * FROM Users WHERE IsActive = 1")
                    .SetCommandTimeout(30))
                .ForMethod("CreateUser", cmd => cmd
                    .UseConnectionAlias("Primary")
                    .UseCommandText("INSERT INTO Users (Name, Email) VALUES (@Name, @Email)")
                    .SetIsolationLevel(IsolationLevel.ReadCommitted))))));
```

### Environment-Specific Configuration

```csharp
var connectionString = builder.Environment.IsDevelopment()
    ? "Server=(localdb)\\MSSQLLocalDB;Database=MyApp;Integrated Security=true;"
    : builder.Configuration.GetConnectionString("Production");

services.UseSyrx(syrx => syrx.UseSqlServer(/* use connectionString */));
```

## Error Handling

### Connection Errors

```csharp
public async Task<User> GetUserAsync(int id)
{
    try
    {
        var users = await _commander.QueryAsync<User>(new { id });
        return users.FirstOrDefault();
    }
    catch (SqlException ex) when (ex.Number == 2) // Timeout
    {
        _logger.LogWarning("Database timeout occurred for user {UserId}", id);
        throw new DataAccessException("Database operation timed out", ex);
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "SQL error occurred while getting user {UserId}", id);
        throw new DataAccessException("Database error occurred", ex);
    }
}
```

### Transaction Errors

Execute operations automatically handle transactions with rollback on error:

```csharp
public async Task<bool> CreateUserAsync(User user)
{
    // Framework automatically wraps in transaction
    // Rolls back on any exception
    return await _commander.ExecuteAsync(user);
}
```

## Best Practices for AI Assistants

### When Generating Code

1. **Follow established patterns**: Use the repository pattern with ICommander<T>
2. **Include proper documentation**: Add XML documentation for all public APIs
3. **Use meaningful names**: Choose descriptive names for classes, methods, and parameters
4. **Handle nulls appropriately**: Use nullable reference types and proper null checking
5. **Consider performance**: Optimize for common scenarios while maintaining readability

### When Modifying Existing Code

1. **Maintain consistency**: Follow existing code style and patterns
2. **Preserve performance**: Don't introduce performance regressions
3. **Update documentation**: Keep XML documentation and README files current
4. **Add tests**: Include appropriate unit and integration tests
5. **Consider breaking changes**: Avoid breaking changes in public APIs

### When Adding New Features

1. **Extend existing patterns**: Build on established architectural patterns
2. **Maintain backwards compatibility**: Preserve existing public APIs
3. **Document thoroughly**: Include comprehensive documentation and examples
4. **Consider all use cases**: Think about edge cases and error scenarios
5. **Benchmark performance**: Ensure new features meet performance standards

---

*This document serves as a comprehensive guide for maintaining consistency and quality in the Syrx.SqlServer codebase. It should be updated as the project evolves and new patterns emerge.*
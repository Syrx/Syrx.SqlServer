# Syrx.SqlServer.Extensions

Dependency injection and configuration extensions for Syrx SQL Server integration.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
  - [Basic Configuration](#basic-configuration)
  - [Advanced Configuration](#advanced-configuration)
- [Configuration Methods](#configuration-methods)
  - [Connection String Management](#connection-string-management)
  - [Command Configuration](#command-configuration)
  - [Service Lifetime](#service-lifetime)
- [Command Builder Methods](#command-builder-methods)
- [Multi-Repository Configuration](#multi-repository-configuration)
- [Repository Registration](#repository-registration)
- [Environment-Specific Configuration](#environment-specific-configuration)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.SqlServer.Extensions` provides easy-to-use extension methods and configuration builders for integrating Syrx with Microsoft SQL Server databases. This package simplifies the setup process and provides fluent APIs for configuration.

## Features

- **Fluent Configuration**: Easy-to-read configuration syntax
- **Dependency Injection**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Connection Management**: Simplified connection string management
- **Command Builders**: Type-safe command configuration
- **Service Lifetime Control**: Configurable service lifetimes

## Installation

```bash
dotnet add package Syrx.SqlServer.Extensions
```

**Package Manager**
```bash
Install-Package Syrx.SqlServer.Extensions
```

**PackageReference**
```xml
<PackageReference Include="Syrx.SqlServer.Extensions" Version="2.4.5" />
```

## Quick Start

### Basic Configuration

```csharp
using Syrx.SqlServer.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseSqlServer(sqlServer => sqlServer
            .AddConnectionString("Default", "Server=localhost;Database=MyDb;Trusted_Connection=true;")
            .AddCommand(types => types
                .ForType<UserRepository>(methods => methods
                    .ForMethod(nameof(UserRepository.GetAllAsync), command => command
                        .UseConnectionAlias("Default")
                        .UseCommandText("SELECT * FROM Users"))))));
}
```

### Advanced Configuration

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Primary", primaryConnectionString)
        .AddConnectionString("ReadOnly", readOnlyConnectionString)
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetActiveUsers", command => command
                    .UseConnectionAlias("Primary")
                    .UseCommandText("SELECT * FROM Users WHERE IsActive = 1")
                    .SetCommandTimeout(30)
                    .SetCommandType(CommandType.Text))
                .ForMethod("GetUserStatistics", command => command
                    .UseConnectionAlias("ReadOnly")  
                    .UseCommandText("EXEC sp_GetUserStats")
                    .SetCommandType(CommandType.StoredProcedure)))
            .ForType<OrderRepository>(methods => methods
                .ForMethod("GetOrdersWithDetails", command => command
                    .UseConnectionAlias("Primary")
                    .UseCommandText(@"
                        SELECT o.*, od.*, p.*
                        FROM Orders o
                        JOIN OrderDetails od ON o.Id = od.OrderId
                        JOIN Products p ON od.ProductId = p.Id
                        WHERE o.CustomerId = @customerId")
                    .SplitOn("Id,Id"))),
        ServiceLifetime.Scoped));
```

## Configuration Methods

### Connection String Management

```csharp
.UseSqlServer(sqlServer => sqlServer
    .AddConnectionString("alias", "connection-string")
    .AddConnectionString("another-alias", "another-connection-string")
)
```

### Command Configuration

```csharp
.AddCommand(types => types
    .ForType<RepositoryType>(methods => methods
        .ForMethod("MethodName", command => command
            .UseConnectionAlias("alias")
            .UseCommandText("SQL command text")
            .SetCommandTimeout(seconds)
            .SetCommandType(CommandType.Text | CommandType.StoredProcedure)
            .SplitOn("column-name-for-multimap")
            .SetIsolationLevel(IsolationLevel.ReadCommitted))))
```

### Service Lifetime

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(/* configuration */),
    ServiceLifetime.Scoped);  // or Singleton, Transient
```

## Command Builder Methods

| Method | Description | Example |
|--------|-------------|---------|
| `UseConnectionAlias(string)` | Specifies which connection string to use | `.UseConnectionAlias("Primary")` |
| `UseCommandText(string)` | Sets the SQL command text | `.UseCommandText("SELECT * FROM Users")` |
| `SetCommandTimeout(int)` | Sets command timeout in seconds | `.SetCommandTimeout(30)` |
| `SetCommandType(CommandType)` | Sets command type | `.SetCommandType(CommandType.StoredProcedure)` |
| `SplitOn(string)` | Sets split columns for multi-map queries | `.SplitOn("Id")` |
| `SetIsolationLevel(IsolationLevel)` | Sets transaction isolation level | `.SetIsolationLevel(IsolationLevel.ReadCommitted)` |

## Multi-Repository Configuration

```csharp
.AddCommand(types => types
    .ForType<UserRepository>(methods => methods
        .ForMethod("GetUsers", command => command
            .UseConnectionAlias("Primary")
            .UseCommandText("SELECT * FROM Users"))
        .ForMethod("GetUserById", command => command
            .UseConnectionAlias("Primary") 
            .UseCommandText("SELECT * FROM Users WHERE Id = @id")))
    .ForType<ProductRepository>(methods => methods
        .ForMethod("GetProducts", command => command
            .UseConnectionAlias("Catalog")
            .UseCommandText("SELECT * FROM Products"))
        .ForMethod("SearchProducts", command => command
            .UseConnectionAlias("Catalog")
            .UseCommandText("SELECT * FROM Products WHERE Name LIKE @search"))))
```

## Repository Registration

Don't forget to register your repositories:

```csharp
services.AddScoped<UserRepository>();
services.AddScoped<ProductRepository>();
```

## Environment-Specific Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = _configuration.GetConnectionString("DefaultConnection");
    
    services.UseSyrx(builder => builder
        .UseSqlServer(sqlServer => sqlServer
            .AddConnectionString("Default", connectionString)
            .AddCommand(/* command configuration */)));
}
```

## Related Packages

- **[Syrx.SqlServer](https://www.nuget.org/packages/Syrx.SqlServer/)**: Core SQL Server provider
- **[Syrx](https://www.nuget.org/packages/Syrx/)**: Core Syrx interfaces
- **[Syrx.Commanders.Databases.Extensions](https://www.nuget.org/packages/Syrx.Commanders.Databases.Extensions/)**: Database framework extensions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Dapper](https://github.com/DapperLib/Dapper)
- SQL Server support provided by [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient)
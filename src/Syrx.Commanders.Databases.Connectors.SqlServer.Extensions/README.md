# Syrx.Commanders.Databases.Connectors.SqlServer.Extensions

Dependency injection extensions for Syrx SQL Server database connectors.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Key Extensions](#key-extensions)
  - [ServiceCollectionExtensions](#servicecollectionextensions)
  - [SqlServerConnectorExtensions](#sqlserverconnectorextensions)
- [Usage](#usage)
  - [Basic Registration](#basic-registration)
  - [Custom Lifetime](#custom-lifetime)
  - [Advanced Configuration](#advanced-configuration)
- [Service Registration Details](#service-registration-details)
- [Service Lifetimes](#service-lifetimes)
  - [Lifetime Recommendations](#lifetime-recommendations)
- [Registration Process](#registration-process)
- [Integration with Other Extensions](#integration-with-other-extensions)
- [Error Handling](#error-handling)
- [Testing Support](#testing-support)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.Commanders.Databases.Connectors.SqlServer.Extensions` provides dependency injection and service registration extensions specifically for SQL Server database connectors in the Syrx framework. This package enables easy registration of SQL Server connectors with DI containers.

## Features

- **Service Registration**: Automatic registration of SQL Server connector services
- **Lifecycle Management**: Configurable service lifetimes for connectors
- **DI Integration**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Builder Pattern**: Fluent configuration APIs
- **Extensibility**: Support for custom connector configurations

## Installation

> **Note**: This package is typically installed automatically as a dependency of `Syrx.SqlServer.Extensions`.

```bash
dotnet add package Syrx.Commanders.Databases.Connectors.SqlServer.Extensions
```

**Package Manager**
```bash
Install-Package Syrx.Commanders.Databases.Connectors.SqlServer.Extensions
```

**PackageReference**
```xml
<PackageReference Include="Syrx.Commanders.Databases.Connectors.SqlServer.Extensions" Version="3.0.0" />
```

## Key Extensions

### ServiceCollectionExtensions

Provides extension methods for `IServiceCollection`:

```csharp
public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddSqlServer(
        this IServiceCollection services, 
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.TryAddToServiceCollection(
            typeof(IDatabaseConnector),
            typeof(SqlServerDatabaseConnector),
            lifetime);
    }
}
```

### SqlServerConnectorExtensions

Provides builder pattern extensions:

```csharp
public static class SqlServerConnectorExtensions
{
    public static SyrxBuilder UseSqlServer(
        this SyrxBuilder builder,
        Action<CommanderSettingsBuilder> factory,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        // Extension implementation
    }
}
```

## Usage

### Basic Registration

```csharp
using Syrx.Commanders.Databases.Connectors.SqlServer.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseSqlServer(sqlServer => sqlServer
            .AddConnectionString("Default", connectionString)
            .AddCommand(/* command configuration */)));
}
```

### Custom Lifetime

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(
        sqlServer => sqlServer.AddConnectionString(/* config */),
        ServiceLifetime.Scoped));
```

### Advanced Configuration

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Primary", primaryConnection)
        .AddConnectionString("ReadOnly", readOnlyConnection)
        .AddCommand(types => types
            .ForType<UserRepository>(methods => methods
                .ForMethod("GetUsers", command => command
                    .UseConnectionAlias("ReadOnly")
                    .UseCommandText("SELECT * FROM Users")))),
        ServiceLifetime.Singleton));
```

## Service Registration Details

The extensions automatically register:

1. **ICommanderSettings**: The configuration settings instance
2. **IDatabaseCommandReader**: For reading command configurations  
3. **IDatabaseConnector**: The SQL Server-specific connector
4. **DatabaseCommander<T>**: The generic database commander

## Service Lifetimes

| Lifetime | Use Case | Description |
|----------|----------|-------------|
| `Transient` | Default | New instance per injection |
| `Scoped` | Web Apps | Instance per request/scope |
| `Singleton` | Performance | Single instance for application |

### Lifetime Recommendations

- **Transient**: Default for most scenarios, minimal overhead
- **Scoped**: Web applications where you want request-scoped connections
- **Singleton**: High-performance scenarios with careful connection management

## Registration Process

When calling `.UseSqlServer()`, the following happens:

1. **Settings Registration**: CommanderSettings configured as transient
2. **Reader Registration**: DatabaseCommandReader registered
3. **Connector Registration**: SqlServerDatabaseConnector registered
4. **Commander Registration**: DatabaseCommander<T> registered

## Integration with Other Extensions

Works seamlessly with other Syrx extension packages:

```csharp
services.UseSyrx(builder => builder
    .UseSqlServer(/* SQL Server config */)
    .UseMySql(/* MySQL config */)     // If needed
    .UseNpgsql(/* PostgreSQL config */)); // If needed
```

## Error Handling

The extensions provide proper error handling for:
- Invalid configuration scenarios
- Missing dependencies
- Circular dependency issues
- Service registration conflicts

## Testing Support

The extensions support testing scenarios:

```csharp
// Test service collection
var services = new ServiceCollection();
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Test", testConnectionString)
        .AddCommand(/* test commands */)));

var provider = services.BuildServiceProvider();
var connector = provider.GetService<IDatabaseConnector>();
```

## Related Packages

- **[Syrx.SqlServer.Extensions](https://www.nuget.org/packages/Syrx.SqlServer.Extensions/)**: High-level SQL Server extensions
- **[Syrx.Commanders.Databases.Connectors.SqlServer](https://www.nuget.org/packages/Syrx.Commanders.Databases.Connectors.SqlServer/)**: Core SQL Server connector
- **[Syrx.Commanders.Databases.Extensions](https://www.nuget.org/packages/Syrx.Commanders.Databases.Extensions/)**: Base database extensions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Microsoft.Extensions.DependencyInjection](https://github.com/dotnet/extensions)
- Follows [Dapper](https://github.com/DapperLib/Dapper) performance patterns

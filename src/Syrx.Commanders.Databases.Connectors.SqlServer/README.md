# Syrx.Commanders.Databases.Connectors.SqlServer

Core SQL Server database connector for the Syrx data access framework.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Architecture](#architecture)
- [Usage](#usage)
- [Configuration](#configuration)
- [Connection Management](#connection-management)
- [Performance Considerations](#performance-considerations)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.Commanders.Databases.Connectors.SqlServer` provides the core SQL Server database connectivity implementation for the Syrx framework. This package contains the fundamental connector class that enables database operations against Microsoft SQL Server instances using the Microsoft.Data.SqlClient library.

## Features

- **SQL Server Integration**: Direct integration with Microsoft SQL Server databases
- **Connection Factory Pattern**: Uses SqlClientFactory for optimal connection creation
- **Base Connector Extensions**: Extends DatabaseConnector for consistent behavior
- **Thread-Safe Operations**: Concurrent connection handling with proper resource management
- **Configuration-Driven**: Supports connection string resolution through ICommanderSettings

## Installation

> **Note**: This package is typically installed as a dependency of higher-level packages like `Syrx.SqlServer.Extensions`.

```bash
dotnet add package Syrx.Commanders.Databases.Connectors.SqlServer
```

**Package Manager**
```bash
Install-Package Syrx.Commanders.Databases.Connectors.SqlServer
```

**PackageReference**
```xml
<PackageReference Include="Syrx.Commanders.Databases.Connectors.SqlServer" Version="3.0.0" />
```

## Architecture

The connector follows the Syrx architecture pattern:

```
SqlServerDatabaseConnector
    ├── Inherits from DatabaseConnector
    ├── Uses SqlClientFactory for connection creation
    ├── Manages connection string resolution
    └── Provides IDbConnection instances for SQL Server
```

### Key Components

- **SqlServerDatabaseConnector**: Main connector class implementing IDatabaseConnector
- **SqlClientFactory Integration**: Uses Microsoft.Data.SqlClient factory pattern
- **Settings Integration**: Leverages ICommanderSettings for configuration

## Usage

### Direct Usage (Advanced)

```csharp
// Direct instantiation (not recommended for normal use)
var settings = new CommanderSettings(/* configuration */);
var connector = new SqlServerDatabaseConnector(settings);

// The connector is typically used internally by DatabaseCommander
```

### Typical Integration

```csharp
// The connector is registered automatically when using extensions
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Default", connectionString)
        // Additional configuration...
    ));

// Injected automatically into DatabaseCommander<T>
public class UserRepository
{
    private readonly ICommander<UserRepository> _commander;
    
    public UserRepository(ICommander<UserRepository> commander)
    {
        _commander = commander; // Uses SqlServerDatabaseConnector internally
    }
}
```

## Configuration

The connector relies on ICommanderSettings for configuration:

### Connection String Configuration

```csharp
var settings = new CommanderSettingsBuilder()
    .AddConnectionString("Primary", "Server=localhost;Database=MyDb;Trusted_Connection=true;")
    .AddConnectionString("ReadOnly", "Server=readonly;Database=MyDb;Trusted_Connection=true;")
    .Build();

var connector = new SqlServerDatabaseConnector(settings);
```

### Command Configuration

Commands are resolved through the settings and used by the connector:

```csharp
var settings = new CommanderSettingsBuilder()
    .AddConnectionString("Default", connectionString)
    .AddCommand("MyApp.Repositories.UserRepository.GetUsers", command => command
        .UseConnectionAlias("Default")
        .UseCommandText("SELECT * FROM Users"))
    .Build();
```

## Connection Management

### Connection Creation

The connector creates connections using the following pattern:

1. **Command Resolution**: Commands are resolved from settings by method name
2. **Connection String Lookup**: Connection strings are resolved by alias
3. **Factory Creation**: SqlClientFactory creates the actual IDbConnection
4. **Resource Management**: Connections are properly disposed after use

### Connection String Requirements

SQL Server connection strings should include:
- **Server/Data Source**: Database server location
- **Database/Initial Catalog**: Target database name  
- **Authentication**: Either Integrated Security or User ID/Password
- **Optional Settings**: Connection timeout, pooling settings, etc.

Example connection strings:
```
Server=localhost;Database=MyDatabase;Integrated Security=true;
Server=myserver;Database=MyDb;User ID=myuser;Password=mypass;Connect Timeout=30;
```

## Performance Considerations

### Connection Pooling

The connector leverages ADO.NET connection pooling automatically:
- Connection strings with identical parameters share pools
- Default pool size is 100 connections
- Configure pooling settings in connection strings:
  ```
  Server=localhost;Database=MyDb;Trusted_Connection=true;Max Pool Size=200;Min Pool Size=10;
  ```

### Thread Safety

- **SqlServerDatabaseConnector**: Thread-safe for concurrent access
- **Connection Creation**: Each call creates a new connection instance
- **Factory Pattern**: SqlClientFactory is thread-safe by design

### Best Practices

1. **Use Connection Pooling**: Ensure connection strings enable pooling
2. **Proper Disposal**: Always dispose connections (handled automatically by framework)
3. **Connection String Caching**: Settings are cached for performance
4. **Avoid Long-Running Connections**: Create connections per operation

## Related Packages

### Dependencies

- **[Syrx.Commanders.Databases.Connectors](https://www.nuget.org/packages/Syrx.Commanders.Databases.Connectors/)**: Base connector abstractions
- **[Syrx.Commanders.Databases.Settings](https://www.nuget.org/packages/Syrx.Commanders.Databases.Settings/)**: Configuration model
- **[Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/)**: SQL Server client library

### Extension Packages

- **[Syrx.Commanders.Databases.Connectors.SqlServer.Extensions](https://www.nuget.org/packages/Syrx.Commanders.Databases.Connectors.SqlServer.Extensions/)**: DI registration extensions
- **[Syrx.SqlServer.Extensions](https://www.nuget.org/packages/Syrx.SqlServer.Extensions/)**: High-level configuration extensions

### Framework Packages

- **[Syrx.Commanders.Databases](https://www.nuget.org/packages/Syrx.Commanders.Databases/)**: Core database command framework
- **[Syrx.SqlServer](https://www.nuget.org/packages/Syrx.SqlServer/)**: Complete SQL Server solution

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient)
- Extends [Syrx.Commanders.Databases.Connectors](https://github.com/Syrx/Syrx.Commanders.Databases) base functionality
- Part of the [Syrx](https://github.com/Syrx/Syrx) data access framework

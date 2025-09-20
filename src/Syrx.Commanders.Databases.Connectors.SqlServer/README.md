# Syrx.Commanders.Databases.Connectors.SqlServer

SQL Server database connector implementation for the Syrx data access framework.

## Overview

`Syrx.Commanders.Databases.Connectors.SqlServer` provides the low-level SQL Server database connector implementation for Syrx. This package contains the core connector logic that manages SQL Server database connections using Microsoft.Data.SqlClient.

## Features

- **SQL Server Connectivity**: Native SQL Server connection management
- **DbProviderFactory Integration**: Uses SqlClientFactory for connection creation
- **Connection Pooling**: Leverages SQL Server connection pooling capabilities
- **Thread-Safe Operations**: Safe for concurrent access
- **Configuration-Driven**: Connection management based on Syrx configuration settings

## Installation

> **Note**: This package is typically installed automatically as a dependency of `Syrx.SqlServer` or `Syrx.SqlServer.Extensions`.

```bash
dotnet add package Syrx.Commanders.Databases.Connectors.SqlServer
```

**Package Manager**
```bash
Install-Package Syrx.Commanders.Databases.Connectors.SqlServer
```

**PackageReference**
```xml
<PackageReference Include="Syrx.Commanders.Databases.Connectors.SqlServer" Version="2.4.5" />
```

## Architecture

This package implements the `IDatabaseConnector` interface specifically for SQL Server:

```csharp
public class SqlServerDatabaseConnector : DatabaseConnector
{
    public SqlServerDatabaseConnector(ICommanderSettings settings) 
        : base(settings, () => SqlClientFactory.Instance)
    {
    }
}
```

## Key Components

### SqlServerDatabaseConnector

The main connector class that:
- Inherits from `DatabaseConnector` base class
- Uses `SqlClientFactory.Instance` for creating SQL Server connections
- Manages connection string resolution based on aliases
- Handles connection lifecycle management

### Connection Creation Process

1. **Alias Resolution**: Resolves connection string alias from command settings
2. **Factory Creation**: Uses SqlClientFactory to create the connection instance
3. **Connection String Assignment**: Assigns the resolved connection string
4. **Connection Return**: Returns the configured IDbConnection

## Usage

This package is typically consumed through higher-level Syrx packages:

```csharp
// Usually configured through extensions
services.UseSyrx(builder => builder
    .UseSqlServer(sqlServer => sqlServer
        .AddConnectionString("Primary", connectionString)
        .AddCommand(/* configuration */)));
```

Direct usage (advanced scenarios):

```csharp
var settings = /* your ICommanderSettings */;
var connector = new SqlServerDatabaseConnector(settings);
var connection = connector.CreateConnection(commandSetting);
```

## Connection String Support

Supports all standard SQL Server connection string formats:

### Integrated Security
```
Server=localhost;Database=MyDatabase;Integrated Security=true;
```

### SQL Server Authentication
```
Server=localhost;Database=MyDatabase;User ID=myuser;Password=mypass;
```

### Named Instances
```
Server=localhost\SQLEXPRESS;Database=MyDatabase;Integrated Security=true;
```

### Azure SQL Database
```
Server=tcp:myserver.database.windows.net,1433;Database=mydatabase;User ID=myuser;Password=mypass;Encrypt=True;
```

## Configuration Requirements

Requires proper `ICommanderSettings` configuration with:
- Connection string settings with aliases
- Command settings that reference those aliases

Example configuration structure:
```csharp
{
    "Connections": [
        {
            "Alias": "Primary",
            "ConnectionString": "Server=localhost;Database=MyDb;Integrated Security=true;"
        }
    ],
    "Namespaces": [
        {
            "Name": "MyApp.Repositories",
            "Types": [
                {
                    "Name": "UserRepository", 
                    "Commands": {
                        "GetUsers": {
                            "ConnectionAlias": "Primary",
                            "CommandText": "SELECT * FROM Users"
                        }
                    }
                }
            ]
        }
    ]
}
```

## Error Handling

The connector handles various error scenarios:
- **Missing Connection Alias**: Throws `NullReferenceException` with descriptive message
- **Invalid Connection String**: SQL Server exceptions are propagated with full context
- **Connection Creation Failure**: Detailed error information is preserved

## Performance Considerations

- **Connection Pooling**: Relies on SQL Server's built-in connection pooling
- **Factory Pattern**: Minimal overhead using singleton SqlClientFactory
- **Resource Management**: Proper disposal pattern implementation
- **Thread Safety**: Safe for concurrent operations

## Related Packages

- **[Syrx.SqlServer](https://www.nuget.org/packages/Syrx.SqlServer/)**: High-level SQL Server provider
- **[Syrx.SqlServer.Extensions](https://www.nuget.org/packages/Syrx.SqlServer.Extensions/)**: Configuration extensions
- **[Syrx.Commanders.Databases.Connectors](https://www.nuget.org/packages/Syrx.Commanders.Databases.Connectors/)**: Base connector abstractions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient)
- Follows [Dapper](https://github.com/DapperLib/Dapper) performance patterns
# Syrx.SqlServer

Microsoft SQL Server database provider for the Syrx data access framework.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
  - [1. Configure Services](#1-configure-services)
  - [2. Create Repository](#2-create-repository)
  - [3. Use in Controllers/Services](#3-use-in-controllersservices)
- [Configuration](#configuration)
  - [Connection Strings](#connection-strings)
  - [Command Configuration](#command-configuration)
- [Multi-map Queries](#multi-map-queries)
- [Transaction Management](#transaction-management)
- [Connection String Requirements](#connection-string-requirements)
- [Performance Tips](#performance-tips)
- [Related Packages](#related-packages)
- [License](#license)
- [Credits](#credits)

## Overview

`Syrx.SqlServer` provides SQL Server database support for Syrx applications. This package enables you to use Syrx's powerful data access patterns with Microsoft SQL Server databases, leveraging the performance and flexibility of Dapper underneath.

## Features

- **SQL Server Integration**: Native support for Microsoft SQL Server databases
- **High Performance**: Built on top of Dapper for optimal performance
- **Transaction Support**: Full transaction management with automatic rollback
- **Multi-map Queries**: Complex object composition with up to 16 input parameters
- **Async Operations**: Full async/await support for all operations

## Installation

> **Recommended**: Install the Extensions package for easier configuration and setup.

```bash
dotnet add package Syrx.SqlServer.Extensions
```

**Core Package Only**
```bash
dotnet add package Syrx.SqlServer
```

**Package Manager**
```bash
Install-Package Syrx.SqlServer.Extensions
Install-Package Syrx.SqlServer
```

**PackageReference**
```xml
<PackageReference Include="Syrx.SqlServer.Extensions" Version="2.4.5" />
<PackageReference Include="Syrx.SqlServer" Version="2.4.5" />
```

## Quick Start

### 1. Configure Services

```csharp
using Syrx.SqlServer.Extensions;

public void ConfigureServices(IServiceCollection services)
{
    services.UseSyrx(builder => builder
        .UseSqlServer(sqlServer => sqlServer
            .AddConnectionString("DefaultConnection", connectionString)
            .AddCommand(types => types
                .ForType<UserRepository>(methods => methods
                    .ForMethod(nameof(UserRepository.GetByIdAsync), command => command
                        .UseConnectionAlias("DefaultConnection")
                        .UseCommandText("SELECT * FROM Users WHERE Id = @id"))))));
}
```

### 2. Create Repository

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

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _commander.QueryAsync<User>();
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _commander.ExecuteAsync(user) ? user : null;
    }
}
```

### 3. Use in Controllers/Services

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserRepository _userRepository;

    public UsersController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
}
```

## Configuration

### Connection Strings

```csharp
.UseSqlServer(sqlServer => sqlServer
    .AddConnectionString("Primary", "Server=localhost;Database=MyDb;Trusted_Connection=true;")
    .AddConnectionString("ReadOnly", "Server=readonly;Database=MyDb;Trusted_Connection=true;")
)
```

### Command Configuration

```csharp
.AddCommand(types => types
    .ForType<UserRepository>(methods => methods
        .ForMethod("GetActiveUsers", command => command
            .UseConnectionAlias("Primary")
            .UseCommandText("SELECT * FROM Users WHERE IsActive = 1")
            .SetCommandTimeout(30))
        .ForMethod("GetUserStats", command => command
            .UseConnectionAlias("ReadOnly")
            .UseCommandText("EXEC GetUserStatistics")
            .SetCommandType(CommandType.StoredProcedure))))
```

## Multi-map Queries

For complex object composition:

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
```

## Transaction Management

Execute operations are automatically wrapped in transactions:

```csharp
public async Task<bool> UpdateUserAsync(User user)
{
    // Automatically handles transaction with rollback on exceptions
    return await _commander.ExecuteAsync(user);
}
```

## Connection String Requirements

SQL Server connection strings should include:
- Server/Data Source
- Database/Initial Catalog  
- Authentication (Integrated Security or User ID/Password)

Example:
```
Server=localhost;Database=MyDatabase;Integrated Security=true;
```

## Performance Tips

- Use parameterized queries for security and performance
- Consider connection pooling settings
- Optimize command timeout values based on query complexity
- Use async methods for I/O-bound operations

## Related Packages

- **[Syrx.SqlServer.Extensions](https://www.nuget.org/packages/Syrx.SqlServer.Extensions/)**: Recommended extensions for easier setup
- **[Syrx](https://www.nuget.org/packages/Syrx/)**: Core Syrx interfaces
- **[Syrx.Commanders.Databases](https://www.nuget.org/packages/Syrx.Commanders.Databases/)**: Database command abstractions

## License

This project is licensed under the [MIT License](https://github.com/Syrx/Syrx/blob/main/LICENSE).

## Credits

- Built on top of [Dapper](https://github.com/DapperLib/Dapper)
- SQL Server support provided by [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient)
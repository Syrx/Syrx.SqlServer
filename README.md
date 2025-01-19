# Syrx.SqlServer

This project provides Syrx support for SqlServer. The overall experience of using [Syrx](https://github.com/Syrx/Syrx) remains the same. The only difference should be during dependency registration. 


## Installation 
> [!TIP]
> We recommend installing the Extensions package which includes extension methods for easier configuration. 

|Source|Command|
|--|--|
|.NET CLI|```dotnet add package Syrx.SqlServer.Extensions```
|Package Manager|```Install-Package Syrx.SqlServer.Extensions```
|Package Reference|```<PackageReference Include="Syrx.SqlServer.Extensions" Version="2.4.0" />```|
|Paket CLI|```paket add Syrx.SqlServer.Extensions --version 2.4.0```|

However, if you don't need the configuration options, you can install the standalone package via [nuget](https://www.nuget.org/packages/Syrx.SqlServer/).  

|Source|Command|
|--|--|
|.NET CLI|```dotnet add package Syrx.SqlServer```
|Package Manager|```Install-Package Syrx.SqlServer```
|Package Reference|```<PackageReference Include="Syrx.SqlServer" Version="2.4.0" />```|
|Paket CLI|```paket add Syrx.SqlServer --version 2.4.0```|


## Extensions
The `Syrx.SqlServer.Extensions` package provides dependency injection support via extension methods. 

```csharp
// add a using statement to the top of the file or in a global usings file.
using Syrx.Commanders.Databases.Connectors.SqlServer.Extensions;

public static IServiceCollection Install(this IServiceCollection services)
{
    return services
        .UseSyrx(factory => factory         // inject Syrx
        .UseSqlServer(builder => builder        // using the SqlServer implementation
            .AddConnectionString(/*...*/)   // add/resolve connection string details 
            .AddCommand(/*...*/)            // add/resolve commands for each type/method
            )
        );
}
```

## Credits
Syrx is inspired by and build on top of [Dapper](https://github.com/DapperLib/Dapper).    
SqlServer support is provided by [Microsoft.Data.SqlClient](https://github.com/dotnet/sqlclient).
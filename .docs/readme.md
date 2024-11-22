# Syrx.SqlServer

This repository hosts Syrx support for SQL Server. This package is a "convenience" package in the that it exists solely as pre-packaged support for SQL Server. It 

## Syrx.SqlServer.Extensions

We recommend installing the extensions package which accompanies this package to support easier configuration. 

``` 
Install-Package Syrx.SqlServer.Extensions
```

### Configuration
Once the extensions package has been installed, you can use these extensions to support configuration. 

```csharp

public static class SyrxInstaller
{
    public static IServiceCollection Install(this IServiceCollection services)
    {
        return services.UseSyrx(factory =>
            factory.UseSqlServer(builder =>
            // build up command settings
        ));
    }
}
```


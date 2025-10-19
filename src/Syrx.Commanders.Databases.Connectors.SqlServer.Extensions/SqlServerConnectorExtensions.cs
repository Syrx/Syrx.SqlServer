// ========================================================================================================================================================
// author      : david sexton (@sextondjc | sextondjc.com)
// modified    : 2020.06.21 (22:10)
// site        : https://www.github.com/syrx
// ========================================================================================================================================================


namespace Syrx.Commanders.Databases.Connectors.SqlServer.Extensions
{
    /// <summary>
    /// Provides extension methods for configuring SQL Server database connectivity with the Syrx framework.
    /// </summary>
    public static class SqlServerConnectorExtensions
    {
        /// <summary>
        /// Configures the Syrx framework to use SQL Server as the database provider with the specified settings.
        /// </summary>
        /// <param name="builder">The <see cref="SyrxBuilder"/> instance to configure.</param>
        /// <param name="factory">An action delegate that configures the <see cref="CommanderSettingsBuilder"/> with connection strings and command mappings.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> for the registered services. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
        /// <returns>The <see cref="SyrxBuilder"/> instance for method chaining.</returns>
        /// <remarks>
        /// This method registers all the necessary services for SQL Server database operations including:
        /// <list type="bullet">
        /// <item><description><see cref="ICommanderSettings"/> - Configuration settings for database operations</description></item>
        /// <item><description>Command reader services for resolving database commands</description></item>
        /// <item><description><see cref="SqlServerDatabaseConnector"/> - SQL Server-specific database connector</description></item>
        /// <item><description>Database commander services for executing commands</description></item>
        /// </list>
        /// The factory delegate allows you to configure connection strings, command mappings, and other database-specific settings.
        /// </remarks>
        /// <example>
        /// <code>
        /// services.UseSyrx(builder => builder
        ///     .UseSqlServer(sqlServer => sqlServer
        ///         .AddConnectionString("Default", connectionString)
        ///         .AddCommand(types => types
        ///             .ForType&lt;UserRepository&gt;(methods => methods
        ///                 .ForMethod("GetUsers", command => command
        ///                     .UseConnectionAlias("Default")
        ///                     .UseCommandText("SELECT * FROM Users"))))));
        /// </code>
        /// </example>
        public static SyrxBuilder UseSqlServer(
            this SyrxBuilder builder,
            Action<CommanderSettingsBuilder> factory,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var options = CommanderSettingsBuilderExtensions.Build(factory);
            builder.ServiceCollection
                .AddTransient<ICommanderSettings, CommanderSettings>(a => options)
                .AddReader(lifetime) // add reader
                .AddSqlServer(lifetime) // add connector
                .AddDatabaseCommander(lifetime);
            
            return builder;
        }
        
    }
}
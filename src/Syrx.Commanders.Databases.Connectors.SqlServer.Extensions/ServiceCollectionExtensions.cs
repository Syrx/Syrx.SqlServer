// ========================================================================================================================================================
// author      : david sexton (@sextondjc | sextondjc.com)
// modified    : 2020.06.21 (21:30)
// site        : https://www.github.com/syrx
// ========================================================================================================================================================

namespace Syrx.Commanders.Databases.Connectors.SqlServer.Extensions
{
    /// <summary>
    /// Provides extension methods for registering SQL Server database connector services with the dependency injection container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the SQL Server database connector with the specified service lifetime.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the registered service. Defaults to <see cref="ServiceLifetime.Transient"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// <remarks>
        /// This method registers the <see cref="SqlServerDatabaseConnector"/> as the implementation of <see cref="IDatabaseConnector"/>
        /// using the TryAddToServiceCollection extension method to avoid duplicate registrations.
        /// </remarks>
        internal static IServiceCollection AddSqlServer(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            return services.TryAddToServiceCollection(
                typeof(IDatabaseConnector),
                typeof(SqlServerDatabaseConnector),
                lifetime);
        }
    }
}
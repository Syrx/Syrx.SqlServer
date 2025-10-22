using Microsoft.Data.SqlClient;
using Syrx.Commanders.Databases.Connectors;
using Syrx.Commanders.Databases.Settings;

namespace Syrx.Commanders.Databases.Connectors.SqlServer
{
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
}

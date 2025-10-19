namespace Syrx.Commanders.Databases.Tests.Integration.DatabaseCommanderTests.SqlServerTests
{
    public class DatabaseBuilder
    {
        private readonly ICommander<DatabaseBuilder> _commander;

        public DatabaseBuilder(ICommander<DatabaseBuilder> commander)
        {
            _commander = commander;
        }



        public DatabaseBuilder ClearTable(string name = "poco")
        {
            Throw<ArgumentNullException>(!string.IsNullOrWhiteSpace(name), nameof(name));
            _commander.Execute(() =>
            {
                return _commander.Query<bool>(new { name });
            });
            return this;
        }

        public DatabaseBuilder Populate()
        {
            ClearTable();
            for (var i = 1; i < 151; i++)
            {
                var entry = new {
                    Name = $"entry {i}",
                    Value = i * 10,
                    Modified = DateTime.Today
                };

                _commander.Execute(entry);
            }

            return this;
        }

        /// <summary>
        /// Validates that the pre-built database schema exists and is properly configured.
        /// This method is used when working with the custom Docker image that has pre-built database objects.
        /// </summary>
        /// <returns>The current DatabaseBuilder instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when required database objects are missing.</exception>
        public DatabaseBuilder ValidatePrebuiltSchema()
        {
            ValidateTablesExist();
            ValidateStoredProceduresExist();
            ValidateDataExists();
            RefreshDataIfNeeded();
            return this;
        }

        public void ValidateTablesExist()
        {
            var requiredTables = new[] { "poco", "identity_test", "bulk_insert", "distributed_transaction" };
            
            foreach (var tableName in requiredTables)
            {
                var exists = _commander.Query<bool>(new { tableName }).SingleOrDefault();
                if (!exists)
                {
                    throw new InvalidOperationException($"Required table '{tableName}' does not exist in the pre-built database schema.");
                }
            }
        }

        public void ValidateStoredProceduresExist()
        {
            var requiredProcedures = new[] 
            { 
                "usp_create_table", 
                "usp_identity_tester", 
                "usp_bulk_insert", 
                "usp_bulk_insert_and_return", 
                "usp_clear_table" 
            };
            
            foreach (var procedureName in requiredProcedures)
            {
                var exists = _commander.Query<bool>(new { procedureName }).SingleOrDefault();
                if (!exists)
                {
                    throw new InvalidOperationException($"Required stored procedure '{procedureName}' does not exist in the pre-built database schema.");
                }
            }
        }

        public void ValidateDataExists()
        {
            var recordCount = _commander.Query<int>().SingleOrDefault();
            Console.WriteLine($"[DatabaseBuilder] Current record count in poco table: {recordCount}");
            
            // If we have unexpected data count, refresh to ensure clean state
            if (recordCount != 150)
            {
                Console.WriteLine($"[DatabaseBuilder] Record count {recordCount} is not the expected 150. Refreshing data to ensure clean test state.");
                RefreshDataIfNeeded();
                
                // Verify refresh worked
                var newCount = _commander.Query<int>().SingleOrDefault();
                Console.WriteLine($"[DatabaseBuilder] After refresh, record count: {newCount}");
                
                if (newCount != 150)
                {
                    throw new InvalidOperationException($"Failed to refresh data to expected state. Expected 150 records, but found {newCount} after refresh.");
                }
            }
        }

        public void RefreshDataIfNeeded()
        {
            Console.WriteLine("[DatabaseBuilder] Refreshing poco table data to ensure clean test state...");
            ClearTable();
            Populate();
            Console.WriteLine("[DatabaseBuilder] Data refresh completed");
        }
    }
}

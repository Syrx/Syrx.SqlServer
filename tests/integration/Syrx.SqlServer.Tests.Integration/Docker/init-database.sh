#!/bin/bash

# Start SQL Server in background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to start
echo "Waiting for SQL Server to start..."
sleep 30s

# Run initialization scripts
echo "Initializing Syrx test database..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -d master -i /opt/mssql-tools/bin/01-create-database.sql
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -d Syrx -i /opt/mssql-tools/bin/02-create-tables.sql
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -d Syrx -i /opt/mssql-tools/bin/03-create-procedures.sql
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -C -d Syrx -i /opt/mssql-tools/bin/04-seed-data.sql

echo "Database initialization completed."

# Keep SQL Server running in foreground
wait
#!/bin/bash

# Wait for SQL Server to start
echo "Waiting for SQL Server to start..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT 1" -C

# Run initialization scripts in order
echo "Running database initialization scripts..."

for script in /docker-entrypoint-initdb.d/*.sql; do
    if [ -f "$script" ]; then
        echo "Executing $script..."
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -i "$script" -C
        if [ $? -eq 0 ]; then
            echo "Successfully executed $script"
        else
            echo "Error executing $script"
            exit 1
        fi
    fi
done

echo "Database initialization completed successfully"
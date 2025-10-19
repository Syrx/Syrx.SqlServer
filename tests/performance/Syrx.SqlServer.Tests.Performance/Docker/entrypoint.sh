#!/bin/bash

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
for i in {1..50}; do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT 1" -C > /dev/null 2>&1
    if [ $? -eq 0 ]; then
        echo "SQL Server is ready!"
        break
    fi
    echo "Waiting for SQL Server... ($i/50)"
    sleep 1
done

# Execute initialization scripts
echo "Running database initialization scripts..."

for script in /docker-entrypoint-initdb.d/*.sql; do
    if [ -f "$script" ]; then
        echo "Executing $(basename "$script")..."
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -i "$script" -C
        if [ $? -eq 0 ]; then
            echo "Successfully executed $(basename "$script")"
        else
            echo "Error executing $(basename "$script")"
            exit 1
        fi
    fi
done

echo "Database initialization completed successfully!"

# Keep SQL Server running in the foreground
wait
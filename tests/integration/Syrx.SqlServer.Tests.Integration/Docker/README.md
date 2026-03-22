# SQL Server Docker Setup for Integration Tests# Syrx SQL Server Docker Integration Tests



This directory contains Docker infrastructure for running Syrx.SqlServer integration tests.This directory contains Docker infrastructure for running Syrx SQL Server integration tests using a containerized SQL Server database.



## Quick Start## Overview



**Important:** Use the provided script to start the database and run initialization:The Docker setup provides a consistent, isolated SQL Server environment for integration testing that mirrors the approach used in Syrx.MySql and Syrx.Npgsql. This ensures consistency across all Syrx database provider implementations.



```powershell## File Structure

# From the test project directory:

.\start-test-database.ps1```

Docker/

# Then run tests per framework:├── docker-compose.yml          # Docker Compose configuration

dotnet test --framework net8.0├── Dockerfile                  # SQL Server container definition

dotnet test --framework net9.0├── init-database.sh            # Database initialization script

```├── init-scripts/               # Database initialization scripts

│   ├── 01-create-database.sql  # Database setup

## Why Per-Framework Testing?│   ├── 02-create-tables.sql    # Test table creation

│   ├── 03-create-procedures.sql # Stored procedures

Both .NET 8.0 and .NET 9.0 tests share the same database. Running them concurrently causes data contamination. Always specify `--framework`:│   └── 04-seed-data.sql        # Test data seeding

└── README.md                   # This file

```bash```

✅ dotnet test --framework net8.0     # Correct

✅ dotnet test --framework net9.0     # Correct## Prerequisites

❌ dotnet test                         # Wrong - runs both concurrently

```- Docker Desktop or Docker Engine installed

- Docker Compose available

## Manual Setup- PowerShell (for Windows users)



If you prefer manual setup:## Starting the Test Database



```powershell### Using Docker Compose

# 1. Start SQL Server

cd Docker```powershell

docker compose up -d# Navigate to the Docker directory

cd tests/integration/Syrx.SqlServer.Tests.Integration/Docker

# 2. Wait for healthy status (~60 seconds)

docker ps# Start the SQL Server container

docker-compose up -d

# 3. Run initialization scripts in order

docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/01-create-database.sql# Check container status

docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/02-create-tables.sqldocker-compose ps

docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/03-create-procedures.sql

docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/04-seed-data.sql# View logs

docker-compose logs syrx-sqlserver-test

# 4. Run tests```

dotnet test --framework net8.0

dotnet test --framework net9.0### Using PowerShell (Windows)

```

```powershell

## Key Difference from MySQL/PostgreSQL# Navigate to the Docker directory

Set-Location "tests\integration\Syrx.SqlServer.Tests.Integration\Docker"

**SQL Server does NOT auto-execute scripts** from `/docker-entrypoint-initdb.d/` like MySQL and PostgreSQL. Scripts must be run manually or via the helper script.

# Start the container

## Connection Detailsdocker-compose up -d

```

- **Host:** localhost

- **Port:** 1433## Connection Details

- **Database:** Syrx

- **User:** saThe SQL Server container is configured with the following connection details:

- **Password:** YourStrong!Passw0rd

- **Connection String:** `Server=localhost,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;`- **Host**: localhost

- **Port**: 1433

## Initialization Scripts- **Database**: Syrx

- **Username**: sa

Scripts in `init-scripts/` execute in order:- **Password**: YourStrong!Passw0rd

- **Connection String**: `Server=localhost,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;`

1. `01-create-database.sql` - Creates Syrx database

2. `02-create-tables.sql` - Creates test tables (poco, identity_test, bulk_insert, distributed_transaction)## Database Schema

3. `03-create-procedures.sql` - Creates stored procedures (usp_create_table, usp_identity_tester, etc.)

4. `04-seed-data.sql` - Seeds 150 test rowsThe initialization scripts create the following tables:



## Troubleshooting### Tables

- `poco` - Main test table with id (INT IDENTITY), name (NVARCHAR), value (DECIMAL), modified (DATETIME)

### "Cannot find object 'poco'"- `identity_test` - Identity testing table with same structure

→ Initialization scripts haven't been run. Use `.\start-test-database.ps1`- `bulk_insert` - Bulk operations table with same structure

- `distributed_transaction` - Distributed transaction testing table with same structure

### Tests expect "entry 1" but get "entry 11"

→ Both frameworks running concurrently. Use `--framework net8.0` or `--framework net9.0`### Stored Procedures

- `usp_create_table` - Dynamic table creation procedure

### Container starts but isn't healthy- `usp_identity_tester` - Identity value testing procedure

→ Wait 60+ seconds for SQL Server initialization- `usp_bulk_insert` - Bulk data insertion procedure

- `usp_bulk_insert_and_return` - Bulk insert with return values

## Cleanup- `usp_clear_table` - Table truncation procedure



```bash## Running Integration Tests

# Stop containers

docker compose downOnce the SQL Server container is running, you can execute the integration tests:



# Remove volumes (complete reset)```powershell

docker compose down -v# Run all integration tests

```dotnet test



## CI/CD# Run with verbose output

dotnet test --verbosity normal

GitHub Actions automatically:```

1. Starts SQL Server service

2. Waits for readiness## Health Checks

3. Executes initialization scripts

4. Runs testsThe container includes health checks that verify SQL Server is ready to accept connections:



For local development, **you must run the initialization script manually**.```powershell

# Check container health
docker-compose ps

# Manual health check
docker exec syrx-sqlserver-test /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT 1"
```

## Troubleshooting

### Container Won't Start
1. Check if port 1433 is already in use: `netstat -an | findstr 1433`
2. Ensure Docker Desktop is running
3. Check Docker logs: `docker-compose logs syrx-sqlserver-test`

### Connection Issues
1. Verify container is healthy: `docker-compose ps`
2. Test connection manually: `docker exec -it syrx-sqlserver-test /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT name FROM sys.databases;"`
3. Check firewall settings if connecting from external machine

### Database Issues
1. Check initialization logs: `docker-compose logs syrx-sqlserver-test`
2. Connect to database and verify schema: `docker exec -it syrx-sqlserver-test /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;"`
3. Verify test data: `docker exec -it syrx-sqlserver-test /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd" -Q "SELECT COUNT(*) FROM poco;"`

## Stopping the Test Database

```powershell
# Stop and remove containers
docker-compose down

# Stop, remove containers, and delete volumes (fresh start)
docker-compose down -v

# Remove all associated images (complete cleanup)
docker-compose down -v --rmi all
```

## Performance Considerations

- Container uses persistent volumes to maintain data between restarts
- Official SQL Server 2022 base image for compatibility
- Health checks ensure database readiness before tests run
- Connection pooling handled by ADO.NET/SqlClient

## Security Notes

- Default password should be changed for production-like environments
- Container runs on localhost only by default
- Database user has full privileges for testing purposes

## Compatibility

This setup is compatible with:
- SQL Server 2022
- .NET 8.0+
- Docker Engine 20.10+
- Docker Compose 2.0+
- Windows, macOS, and Linux development environments

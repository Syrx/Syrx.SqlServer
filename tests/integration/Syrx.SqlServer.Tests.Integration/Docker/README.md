# Syrx SQL Server Test Database Docker Setup

This Docker setup provides a pre-configured SQL Server instance with all necessary database objects for running the `Syrx.SqlServer.Tests.Integration` test suite.

## Features

- **SQL Server 2022 Developer Edition** - Latest SQL Server version
- **Pre-configured Test Database** - Creates `Syrx` database with all required tables
- **Test Data Seeded** - Populates tables with 150 test records
- **Stored Procedures** - All required test procedures are pre-created
- **Health Checks** - Container health monitoring included

## Database Objects Created

### Tables
- **`poco`** - Main test table with columns: `id`, `name`, `value`, `modified`
- **`identity_test`** - For identity/auto-increment testing
- **`bulk_insert`** - For bulk operation testing
- **`distributed_transaction`** - For distributed transaction testing

### Stored Procedures
- **`usp_create_table`** - Creates tables dynamically for tests
- **`usp_identity_tester`** - Tests identity column functionality
- **`usp_bulk_insert`** - Performs bulk insert operations
- **`usp_bulk_insert_and_return`** - Bulk insert with result return
- **`usp_clear_table`** - Truncates specified tables

## Quick Start

### Using Docker Compose (Recommended)

1. Navigate to this directory:
   ```powershell
   cd "c:\Projects\Syrx\Syrx.SqlServer\tests\integration\Syrx.SqlServer.Tests.Integration\Docker"
   ```

2. Start the container:
   ```powershell
   docker-compose up -d
   ```

3. Wait for initialization to complete (about 60 seconds):
   ```powershell
   docker-compose logs -f syrx-sqlserver-test
   ```

4. When ready, the connection string will be:
   ```
   Server=localhost,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;
   ```

### Using Docker Build

1. Build the image:
   ```powershell
   docker build -t syrx-sqlserver-test .
   ```

2. Run the container:
   ```powershell
   docker run -d -p 1433:1433 --name syrx-sqlserver-test syrx-sqlserver-test
   ```

## Configuration

### Environment Variables

- **`MSSQL_SA_PASSWORD`** - SA user password (default: `YourStrong!Passw0rd`)
- **`ACCEPT_EULA`** - Accept SQL Server EULA (set to `Y`)
- **`MSSQL_PID`** - SQL Server edition (default: `Developer`)

### Connection Details

- **Server**: `localhost,1433`
- **Database**: `Syrx`
- **Username**: `sa`
- **Password**: `YourStrong!Passw0rd`
- **Trust Server Certificate**: `true`

## Test Integration

### Updating Test Configuration

To use this Docker instance in your integration tests, update your test configuration to use:

```csharp
var connectionString = "Server=localhost,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;";
```

### Health Check

The container includes health checks to ensure SQL Server is ready:

```powershell
docker-compose ps
```

Look for `healthy` status before running tests.

## Management Commands

### View Logs
```powershell
docker-compose logs syrx-sqlserver-test
```

### Connect to Database
```powershell
docker exec -it syrx-sqlserver-test /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong!Passw0rd"
```

### Stop and Remove
```powershell
docker-compose down
```

### Stop and Remove with Data
```powershell
docker-compose down -v
```

## Data Persistence

The Docker Compose configuration includes a named volume (`syrx_sqlserver_data`) to persist database files between container restarts. To start fresh, remove the volume:

```powershell
docker-compose down -v
docker-compose up -d
```

## Customization

### Changing the Password

1. Update the password in `docker-compose.yml`:
   ```yaml
   environment:
     - MSSQL_SA_PASSWORD=YourNewPassword!
   ```

2. Update the health check command:
   ```yaml
   test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourNewPassword! -Q 'SELECT 1' || exit 1"]
   ```

### Adding Additional Data

Modify the `04-seed-data.sql` file to include additional test data or tables as needed.

## Troubleshooting

### Container Won't Start
- Ensure port 1433 is not already in use
- Check Docker Desktop is running
- Verify password meets SQL Server complexity requirements

### Database Not Ready
- Wait for health check to show `healthy` status
- Check logs for initialization progress
- Allow up to 2 minutes for full initialization

### Connection Issues
- Verify TrustServerCertificate=true is set
- Check firewall settings
- Ensure SQL Server Browser service is not interfering

## File Structure

```
Docker/
├── Dockerfile                          # Main Docker image definition
├── docker-compose.yml                  # Docker Compose configuration
├── init-database.sh                   # Database initialization script
├── README.md                          # This file
└── init-scripts/
    ├── 01-create-database.sql         # Creates Syrx database
    ├── 02-create-tables.sql           # Creates all test tables
    ├── 03-create-procedures.sql       # Creates stored procedures
    └── 04-seed-data.sql              # Seeds test data
```

This setup ensures that your `Syrx.SqlServer.Tests.Integration` tests have a consistent, isolated database environment that's ready to use immediately after container startup.
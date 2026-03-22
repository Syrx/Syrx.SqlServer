# Syrx.SqlServer Integration Testing

## Quick Start

```powershell
# 1. Start SQL Server and initialize database
.\start-test-database.ps1

# 2. Run tests per framework
dotnet test --framework net8.0
dotnet test --framework net9.0
```

That's it! The script handles everything automatically.

## Important: Per-Framework Testing Required

This project targets both .NET 8.0 and .NET 9.0 and uses a **shared database**. Running both frameworks concurrently causes data contamination and test failures.

### ✅ Correct Usage

```bash
# Run each framework separately
dotnet test --framework net8.0
dotnet test --framework net9.0

# Or run both sequentially
dotnet test --framework net8.0 && dotnet test --framework net9.0
```

### ❌ Incorrect Usage

```bash
# This runs BOTH frameworks concurrently - tests will fail!
dotnet test
```

**Why?** When you run `dotnet test` without `--framework`, it spawns **two separate processes** (one for net8.0, one for net9.0) that run in parallel. Both processes modify the same database tables simultaneously, causing race conditions like:

- Expected: `"entry 1"` → Actual: `"entry 11"` (second framework saw modified data)
- Transaction tests fail with wrong row counts
- Isolation tests fail due to cross-framework interference

## Understanding the Test Setup

### Database Configuration

- **Single Database:** Tests use one shared database named `Syrx`
- **Single Instance:** One SQL Server container serves both frameworks
- **Sequential Execution Within Framework:** xUnit runs tests sequentially within each framework (via `xunit.runner.json`)
- **But:** xUnit cannot coordinate between separate dotnet processes (net8.0 vs net9.0)

### Why Not Use Separate Databases?

SQL Server doesn't auto-execute initialization scripts like MySQL/PostgreSQL. Maintaining separate databases per framework would require:
- Complex framework detection logic
- Duplicate init scripts for each database
- Manual coordination between frameworks

The **simple solution** is to run one framework at a time.

## SQL Server Initialization

### Key Difference from MySQL/PostgreSQL

**SQL Server containers do NOT auto-execute scripts** from `/docker-entrypoint-initdb.d/`. The `start-test-database.ps1` script handles this by:

1. Starting the SQL Server container
2. Waiting for health checks (~60-70 seconds)
3. Executing initialization scripts via `sqlcmd`
4. Confirming successful setup

### Manual Initialization (if needed)

```bash
# 1. Start container
cd Docker
docker compose up -d

# 2. Wait for healthy status
docker ps

# 3. Run init scripts in order
docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/01-create-database.sql
docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/02-create-tables.sql
docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/03-create-procedures.sql
docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourStrong!Passw0rd' -C -i /docker-entrypoint-initdb.d/04-seed-data.sql
```

## Helper Script Options

```powershell
# Standard startup with initialization
.\start-test-database.ps1

# Clean start (removes volumes, fresh database)
.\start-test-database.ps1 -Clean

# Start without initialization (useful if already initialized)
.\start-test-database.ps1 -NoInit

# Get help
Get-Help .\start-test-database.ps1 -Detailed
```

## Connection Details

- **Host:** localhost
- **Port:** 1433
- **Database:** Syrx
- **Username:** sa
- **Password:** YourStrong!Passw0rd
- **Connection String:** `Server=localhost,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;`

## Common Issues

### "Cannot find the object 'poco'"

**Cause:** Database not initialized  
**Solution:** Run `.\start-test-database.ps1`

### Tests expect "entry 1" but get "entry 11" (or similar)

**Cause:** Both frameworks running concurrently  
**Solution:** Always use `--framework net8.0` or `--framework net9.0`

### Container starts but isn't healthy after 2 minutes

**Cause:** SQL Server needs significant resources  
**Solution:** 
- Ensure Docker has at least 2GB RAM allocated
- Wait longer (SQL Server can take 60+ seconds)
- Check logs: `docker logs syrx-sqlserver-test`

### Script fails with "Failed to execute initialization script"

**Cause:** Script syntax error or database state issue  
**Solution:** Run `.\start-test-database.ps1 -Clean` to start fresh

## CI/CD

GitHub Actions workflow automatically:
1. Starts SQL Server as a service
2. Waits for readiness
3. Executes all initialization scripts
4. Runs tests (all frameworks run concurrently in CI - separate containers per framework)

**Local development requires manual initialization** via the helper script.

## Performance Notes

- **First run:** ~70 seconds for SQL Server to become healthy
- **Subsequent runs:** ~5-10 seconds (container already initialized)
- **Test execution:** ~40-50 seconds per framework (210 total tests)
- **Sequential overhead:** Minimal (tests within framework already sequential)

## Database Schema

### Tables
- `poco` - Main test table (150 rows)
- `identity_test` - Identity value testing
- `bulk_insert` - Bulk operation testing
- `distributed_transaction` - Transaction testing

### Stored Procedures
- `usp_create_table` - Dynamic table creation
- `usp_identity_tester` - Identity value testing
- `usp_bulk_insert` - Bulk data operations
- `usp_bulk_insert_and_return` - Bulk ops with return values
- `usp_clear_table` - Table cleanup

## Cleanup

```bash
# Stop container (preserves data)
docker compose -f Docker/docker-compose.yml down

# Complete cleanup (removes volumes)
docker compose -f Docker/docker-compose.yml down -v
```

## Comparison with MySQL/PostgreSQL

| Feature | SQL Server | MySQL/PostgreSQL |
|---------|-----------|------------------|
| Auto-init scripts | ❌ No | ✅ Yes |
| Startup time | ~60-70s | ~10-15s |
| Helper script needed | ✅ Yes | ❌ No |
| Multi-framework | Same approach | Same approach |

## Additional Resources

- [Docker Setup Details](Docker/README.md)
- [xUnit Configuration](xunit.runner.json)
- [Syrx Documentation](https://github.com/Syrx)

# Initialize SQL Server databases for Syrx integration tests
# This script runs the init scripts in order

$ErrorActionPreference = "Stop"

$scripts = @(
    "01-create-database.sql",
    "02-create-tables.sql",
    "03-create-procedures.sql",
    "04-seed-data.sql"
)

$scriptPath = Join-Path $PSScriptRoot "init-scripts"

Write-Host "Initializing Syrx SQL Server test databases..." -ForegroundColor Cyan

foreach ($script in $scripts) {
    $fullPath = Join-Path $scriptPath $script
    Write-Host "Executing: $script" -ForegroundColor Yellow
    
    # Use sqlcmd with -C for trust server certificate
    docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd `
        -S localhost `
        -U sa `
        -P 'YourStrong!Passw0rd' `
        -C `
        -d master `
        -i "/docker-entrypoint-initdb.d/$script"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to execute $script"
        exit 1
    }
}

Write-Host "Database initialization complete!" -ForegroundColor Green

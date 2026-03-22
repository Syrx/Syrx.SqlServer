#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the SQL Server test database and initializes it with test data.

.DESCRIPTION
    This script starts the SQL Server Docker container and runs the initialization
    scripts to set up the test database. Run this before executing integration tests.

.EXAMPLE
    .\start-test-database.ps1
    Starts the SQL Server container and initializes the database.

.EXAMPLE
    .\start-test-database.ps1 -Clean
    Stops and removes existing containers, then starts fresh.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [switch]$Clean,
    
    [Parameter(Mandatory=$false)]
    [switch]$NoInit
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dockerDir = Join-Path $scriptDir "Docker"
$initScriptsDir = Join-Path $dockerDir "init-scripts"

Write-Host "SQL Server Test Database Initialization" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Change to Docker directory
Push-Location $dockerDir

try {
    if ($Clean) {
        Write-Host "Stopping and removing existing containers..." -ForegroundColor Yellow
        docker compose down -v
        Write-Host ""
    }

    Write-Host "Starting SQL Server container..." -ForegroundColor Green
    docker compose up -d
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to start Docker containers"
    }
    
    Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
    
    # Wait for health check
    $maxAttempts = 60
    $attempt = 0
    $ready = $false
    
    while ($attempt -lt $maxAttempts -and -not $ready) {
        $attempt++
        Start-Sleep -Seconds 2
        
        $health = docker inspect --format='{{.State.Health.Status}}' syrx-sqlserver-test 2>$null
        
        if ($health -eq "healthy") {
            $ready = $true
            Write-Host "SQL Server is healthy!" -ForegroundColor Green
        } else {
            Write-Host "Waiting for SQL Server... (attempt $attempt/$maxAttempts, status: $health)" -ForegroundColor Gray
        }
    }
    
    if (-not $ready) {
        throw "SQL Server did not become healthy within the expected time"
    }
    
    if (-not $NoInit) {
        Write-Host ""
        Write-Host "Initializing database..." -ForegroundColor Green
        
        # Get all SQL scripts in order
        $scripts = Get-ChildItem -Path $initScriptsDir -Filter "*.sql" | Sort-Object Name
        
        foreach ($script in $scripts) {
            Write-Host "  Running: $($script.Name)" -ForegroundColor Cyan
            
            $result = docker exec syrx-sqlserver-test /opt/mssql-tools18/bin/sqlcmd `
                -S localhost `
                -U sa `
                -P 'YourStrong!Passw0rd' `
                -C `
                -i "/docker-entrypoint-initdb.d/$($script.Name)" 2>&1
            
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Error executing $($script.Name):" -ForegroundColor Red
                Write-Host $result
                throw "Failed to execute initialization script: $($script.Name)"
            }
            
            Write-Host "    ✓ Success" -ForegroundColor Green
        }
        
        Write-Host ""
        Write-Host "Database initialization complete!" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "SQL Server is ready for testing!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Connection details:" -ForegroundColor Cyan
    Write-Host "  Host:     localhost" -ForegroundColor White
    Write-Host "  Port:     1433" -ForegroundColor White
    Write-Host "  Database: Syrx" -ForegroundColor White
    Write-Host "  Username: sa" -ForegroundColor White
    Write-Host "  Password: YourStrong!Passw0rd" -ForegroundColor White
    Write-Host ""
    Write-Host "To run tests:" -ForegroundColor Cyan
    Write-Host "  dotnet test --framework net8.0" -ForegroundColor White
    Write-Host "  dotnet test --framework net9.0" -ForegroundColor White
    Write-Host ""
    Write-Host "To stop the database:" -ForegroundColor Cyan
    Write-Host "  docker compose down" -ForegroundColor White
    Write-Host ""
    
} finally {
    Pop-Location
}

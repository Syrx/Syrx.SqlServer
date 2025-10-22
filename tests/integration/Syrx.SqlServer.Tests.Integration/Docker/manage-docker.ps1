# Syrx SQL Server Test Database Docker Management Script

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("build", "up", "down", "restart", "logs", "status", "clean")]
    [string]$Action = "up",
    
    [Parameter(Mandatory=$false)]
    [switch]$Follow,
    
    [Parameter(Mandatory=$false)]
    [switch]$RemoveVolumes
)

$ErrorActionPreference = "Stop"

# Get the directory where this script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "Syrx SQL Server Test Database Docker Management" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

switch ($Action) {
    "build" {
        Write-Host "Building Syrx SQL Server test image..." -ForegroundColor Yellow
        docker-compose build
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Build completed successfully!" -ForegroundColor Green
        } else {
            Write-Host "Build failed!" -ForegroundColor Red
            exit 1
        }
    }
    
    "up" {
        Write-Host "Starting Syrx SQL Server test container..." -ForegroundColor Yellow
        docker-compose up -d
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Container started successfully!" -ForegroundColor Green
            Write-Host ""
            Write-Host "Connection Details:" -ForegroundColor Cyan
            Write-Host "  Server: localhost,1433" -ForegroundColor White
            Write-Host "  Database: Syrx" -ForegroundColor White
            Write-Host "  Username: sa" -ForegroundColor White
            Write-Host "  Password: YourStrong!Passw0rd" -ForegroundColor White
            Write-Host ""
            Write-Host "Connection String:" -ForegroundColor Cyan
            Write-Host "  Server=localhost,1433;Database=Syrx;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=true;" -ForegroundColor White
            Write-Host ""
            Write-Host "Waiting for database initialization..." -ForegroundColor Yellow
            Write-Host "This may take up to 60 seconds..." -ForegroundColor Yellow
            
            # Wait for health check
            $timeout = 120
            $elapsed = 0
            do {
                Start-Sleep 5
                $elapsed += 5
                $status = docker-compose ps --format json | ConvertFrom-Json | Where-Object { $_.Service -eq "syrx-sqlserver-test" } | Select-Object -ExpandProperty Health -ErrorAction SilentlyContinue
                Write-Host "." -NoNewline -ForegroundColor Yellow
                
                if ($elapsed -ge $timeout) {
                    Write-Host ""
                    Write-Host "Timeout waiting for container to become healthy." -ForegroundColor Red
                    Write-Host "Check logs with: .\manage-docker.ps1 logs" -ForegroundColor Yellow
                    break
                }
            } while ($status -ne "healthy")
            
            if ($status -eq "healthy") {
                Write-Host ""
                Write-Host "Database is ready for connections!" -ForegroundColor Green
            }
        } else {
            Write-Host "Failed to start container!" -ForegroundColor Red
            exit 1
        }
    }
    
    "down" {
        Write-Host "Stopping Syrx SQL Server test container..." -ForegroundColor Yellow
        if ($RemoveVolumes) {
            docker-compose down -v
            Write-Host "Container stopped and volumes removed!" -ForegroundColor Green
        } else {
            docker-compose down
            Write-Host "Container stopped!" -ForegroundColor Green
        }
    }
    
    "restart" {
        Write-Host "Restarting Syrx SQL Server test container..." -ForegroundColor Yellow
        docker-compose restart
        Write-Host "Container restarted!" -ForegroundColor Green
    }
    
    "logs" {
        Write-Host "Showing container logs..." -ForegroundColor Yellow
        if ($Follow) {
            docker-compose logs -f syrx-sqlserver-test
        } else {
            docker-compose logs syrx-sqlserver-test
        }
    }
    
    "status" {
        Write-Host "Container status:" -ForegroundColor Yellow
        docker-compose ps
        Write-Host ""
        Write-Host "Health status:" -ForegroundColor Yellow
        $status = docker-compose ps --format json | ConvertFrom-Json | Where-Object { $_.Service -eq "syrx-sqlserver-test" } | Select-Object Name, State, Health
        $status | Format-Table -AutoSize
    }
    
    "clean" {
        Write-Host "Cleaning up all Syrx SQL Server test resources..." -ForegroundColor Yellow
        docker-compose down -v --remove-orphans
        docker system prune -f
        Write-Host "Cleanup completed!" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Available commands:" -ForegroundColor Cyan
Write-Host "  build    - Build the Docker image" -ForegroundColor White
Write-Host "  up       - Start the container (default)" -ForegroundColor White
Write-Host "  down     - Stop the container" -ForegroundColor White
Write-Host "  restart  - Restart the container" -ForegroundColor White
Write-Host "  logs     - Show container logs (use -Follow for live logs)" -ForegroundColor White
Write-Host "  status   - Show container status" -ForegroundColor White
Write-Host "  clean    - Remove all containers, volumes, and images" -ForegroundColor White
Write-Host ""
Write-Host "Examples:" -ForegroundColor Cyan
Write-Host "  .\manage-docker.ps1 up" -ForegroundColor White
Write-Host "  .\manage-docker.ps1 logs -Follow" -ForegroundColor White
Write-Host "  .\manage-docker.ps1 down -RemoveVolumes" -ForegroundColor White
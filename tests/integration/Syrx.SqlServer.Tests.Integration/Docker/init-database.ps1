# Initialize SQL Server database with init scripts
# This script is run from the GitHub Actions workflow or locally to initialize the test database

param(
    [string]$ServerHost = "localhost",
    [int]$ServerPort = 1433,
    [string]$User = "sa",
    [string]$Password = "YourStrong!Passw0rd",
    [string]$ScriptsPath = "./init-scripts"
)

Write-Host "Initializing SQL Server database..."
Write-Host "Server: $ServerHost:$ServerPort"
Write-Host "Scripts path: $ScriptsPath"

# Get all SQL scripts in order
$scripts = Get-ChildItem -Path $ScriptsPath -Filter "*.sql" | Sort-Object Name

foreach ($script in $scripts) {
    Write-Host "Executing script: $($script.Name)"
    
    $content = Get-Content -Path $script.FullName -Raw
    
    # Use sqlcmd to execute the script
    # -C flag trusts server certificate (needed for local development)
    $result = & sqlcmd -S "$ServerHost,$ServerPort" -U $User -P $Password -C -Q $content
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to execute script: $($script.Name)"
        exit 1
    }
    
    Write-Host "Successfully executed: $($script.Name)"
}

Write-Host "Database initialization completed successfully!"

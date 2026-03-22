# This script runs all .sql files in Docker/init-scripts against the SQL Server container
param(
    [string]$SqlServerHost = "localhost",
    [int]$SqlServerPort = 1433,
    [string]$Database = "Syrx",
    [string]$User = "sa",
    [string]$Password = "YourStrong!Passw0rd",
    [string]$ScriptsPath = "../Docker/init-scripts"
)

$ErrorActionPreference = "Stop"

# Find all .sql files, ordered by name
$scriptFiles = Get-ChildItem -Path $ScriptsPath -Filter *.sql | Sort-Object Name

foreach ($file in $scriptFiles) {
    Write-Host "Running script: $($file.Name)"
    $sqlcmdArgs = @(
        "-S", "$SqlServerHost,$SqlServerPort",
        "-U", $User,
        "-P", $Password,
        "-d", $Database,
        "-i", $file.FullName,
        "-b", # Stop on error
        "-I"  # Enable quoted identifiers
    )
    sqlcmd @sqlcmdArgs
}
Write-Host "All scripts executed."

# Create a flag file to signal completion
$flagPath = Join-Path $ScriptsPath 'init.done'
New-Item -ItemType File -Path $flagPath -Force | Out-Null

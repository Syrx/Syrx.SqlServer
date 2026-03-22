<#
.SYNOPSIS
Measures documentation quality across the solution using objective metrics.

.DESCRIPTION
Calculates XML documentation coverage for public C# declarations and README
coverage for projects under src. Produces a weighted composite score.

.PARAMETER RootPath
Repository root path. Defaults to the current working directory.

.PARAMETER MinimumScore
Minimum acceptable composite score from 0 to 100.

.PARAMETER EnforceMinimum
When specified, exits with code 1 if the composite score is below MinimumScore.

.PARAMETER OutputJsonPath
Optional path to write the full metrics payload as JSON.

.EXAMPLE
./scripts/Get-DocumentationMetrics.ps1

.EXAMPLE
./scripts/Get-DocumentationMetrics.ps1 -MinimumScore 85 -EnforceMinimum -OutputJsonPath ./.docs/reports/doc-metrics.json

.OUTPUTS
System.Object
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateNotNullOrEmpty()]
    [string]$RootPath = (Get-Location).Path,

    [Parameter()]
    [ValidateRange(0, 100)]
    [double]$MinimumScore = 80,

    [Parameter()]
    [switch]$EnforceMinimum,

    [Parameter()]
    [string]$OutputJsonPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-Percentage {
    param(
        [Parameter(Mandatory = $true)]
        [double]$Numerator,
        [Parameter(Mandatory = $true)]
        [double]$Denominator
    )

    if ($Denominator -le 0) {
        return 100.0
    }

    return [Math]::Round(($Numerator / $Denominator) * 100.0, 2)
}

function Test-IsPublicDeclarationLine {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Line
    )

    $trimmed = $Line.Trim()

    if (-not $trimmed.StartsWith('public ')) {
        return $false
    }

    if ($trimmed.StartsWith('public namespace ') -or $trimmed.StartsWith('public using ')) {
        return $false
    }

    if ($trimmed -match '^public\s+\w+\s*=>') {
        return $false
    }

    if ($trimmed -match '^public\s+\w+\s*=') {
        return $false
    }

    return $true
}

function Test-HasXmlDocAboveLine {
    param(
        [Parameter()]
        [AllowNull()]
        [string[]]$Lines,
        [Parameter(Mandatory = $true)]
        [int]$Index
    )

    if ($null -eq $Lines -or $Lines.Length -eq 0) {
        return $false
    }

    $cursor = $Index - 1
    while ($cursor -ge 0 -and [string]::IsNullOrWhiteSpace($Lines[$cursor])) {
        $cursor--
    }

    while ($cursor -ge 0 -and $Lines[$cursor].Trim().StartsWith('[')) {
        $cursor--
        while ($cursor -ge 0 -and [string]::IsNullOrWhiteSpace($Lines[$cursor])) {
            $cursor--
        }
    }

    if ($cursor -lt 0) {
        return $false
    }

    return $Lines[$cursor].Trim().StartsWith('///')
}

function Get-PublicDeclarationCoverage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $sourceRoot = Join-Path -Path $RepositoryRoot -ChildPath 'src'
    if (-not (Test-Path -Path $sourceRoot -PathType Container)) {
        throw "Expected source folder not found: $sourceRoot"
    }

    $files = Get-ChildItem -Path $sourceRoot -Recurse -File -Filter '*.cs' |
        Where-Object {
            $_.FullName -notmatch '[\\/]bin[\\/]' -and
            $_.FullName -notmatch '[\\/]obj[\\/]' -and
            $_.FullName -notmatch '[\\/]\.submodules[\\/]'
        }

    $totalPublicDeclarations = 0
    $documentedPublicDeclarations = 0
    $undocumented = @()

    foreach ($file in $files) {
        $lines = Get-Content -Path $file.FullName

        for ($i = 0; $i -lt $lines.Length; $i++) {
            if (-not (Test-IsPublicDeclarationLine -Line $lines[$i])) {
                continue
            }

            $totalPublicDeclarations++

            if (Test-HasXmlDocAboveLine -Lines $lines -Index $i) {
                $documentedPublicDeclarations++
            } else {
                $relativePath = [System.IO.Path]::GetRelativePath($RepositoryRoot, $file.FullName)
                $undocumented += [PSCustomObject]@{
                    Path = $relativePath
                    Line = $i + 1
                    Declaration = $lines[$i].Trim()
                }
            }
        }
    }

    [PSCustomObject]@{
        TotalPublicDeclarations = $totalPublicDeclarations
        DocumentedPublicDeclarations = $documentedPublicDeclarations
        CoveragePercent = Get-Percentage -Numerator $documentedPublicDeclarations -Denominator $totalPublicDeclarations
        Undocumented = $undocumented
    }
}

function Get-ProjectReadmeCoverage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryRoot
    )

    $sourceRoot = Join-Path -Path $RepositoryRoot -ChildPath 'src'
    $projects = Get-ChildItem -Path $sourceRoot -Recurse -File -Filter '*.csproj'

    $projectReadme = foreach ($project in $projects) {
        $projectDirectory = Split-Path -Path $project.FullName -Parent
        $readmePath = Join-Path -Path $projectDirectory -ChildPath 'README.md'
        [PSCustomObject]@{
            Project = [System.IO.Path]::GetRelativePath($RepositoryRoot, $project.FullName)
            HasReadme = Test-Path -Path $readmePath -PathType Leaf
            ReadmePath = [System.IO.Path]::GetRelativePath($RepositoryRoot, $readmePath)
        }
    }

    $withReadme = ($projectReadme | Where-Object { $_.HasReadme }).Count
    $totalProjects = $projectReadme.Count

    [PSCustomObject]@{
        TotalProjects = $totalProjects
        ProjectsWithReadme = $withReadme
        CoveragePercent = Get-Percentage -Numerator $withReadme -Denominator $totalProjects
        ProjectReadme = $projectReadme
    }
}

$resolvedRoot = (Resolve-Path -Path $RootPath).Path
$xmlCoverage = Get-PublicDeclarationCoverage -RepositoryRoot $resolvedRoot
$readmeCoverage = Get-ProjectReadmeCoverage -RepositoryRoot $resolvedRoot

$weights = [PSCustomObject]@{
    PublicXmlDocumentation = 0.8
    ProjectReadmeCoverage = 0.2
}

$compositeScore = [Math]::Round(
    ($xmlCoverage.CoveragePercent * $weights.PublicXmlDocumentation) +
    ($readmeCoverage.CoveragePercent * $weights.ProjectReadmeCoverage),
    2
)

$result = [PSCustomObject]@{
    TimestampUtc = [DateTime]::UtcNow.ToString('o')
    RootPath = $resolvedRoot
    Weights = $weights
    MinimumScore = $MinimumScore
    CompositeScore = $compositeScore
    Metrics = [PSCustomObject]@{
        PublicXmlDocumentation = $xmlCoverage
        ProjectReadmeCoverage = $readmeCoverage
    }
}

Write-Host ''
Write-Host 'Documentation Metrics'
Write-Host '---------------------'
Write-Host ("Public XML coverage: {0}% ({1}/{2})" -f $xmlCoverage.CoveragePercent, $xmlCoverage.DocumentedPublicDeclarations, $xmlCoverage.TotalPublicDeclarations)
Write-Host ("Project README coverage: {0}% ({1}/{2})" -f $readmeCoverage.CoveragePercent, $readmeCoverage.ProjectsWithReadme, $readmeCoverage.TotalProjects)
Write-Host ("Composite score: {0}" -f $compositeScore)

if ($xmlCoverage.Undocumented.Count -gt 0) {
    Write-Host ''
    Write-Host 'Top undocumented declarations (first 25):'
    $xmlCoverage.Undocumented |
        Select-Object -First 25 |
        ForEach-Object {
            Write-Host ("- {0}:{1} :: {2}" -f $_.Path, $_.Line, $_.Declaration)
        }
}

if (-not [string]::IsNullOrWhiteSpace($OutputJsonPath)) {
    $resolvedOutput = if ([System.IO.Path]::IsPathRooted($OutputJsonPath)) {
        $OutputJsonPath
    } else {
        Join-Path -Path $resolvedRoot -ChildPath $OutputJsonPath
    }

    $outputDirectory = Split-Path -Path $resolvedOutput -Parent
    if (-not [string]::IsNullOrWhiteSpace($outputDirectory) -and -not (Test-Path -Path $outputDirectory -PathType Container)) {
        New-Item -Path $outputDirectory -ItemType Directory -Force | Out-Null
    }

    $result | ConvertTo-Json -Depth 8 | Set-Content -Path $resolvedOutput -Encoding utf8
    Write-Host ("JSON report written: {0}" -f [System.IO.Path]::GetRelativePath($resolvedRoot, $resolvedOutput))
}

if ($EnforceMinimum -and $compositeScore -lt $MinimumScore) {
    Write-Error ("Documentation score {0} is below minimum threshold {1}." -f $compositeScore, $MinimumScore)
    exit 1
}

$result
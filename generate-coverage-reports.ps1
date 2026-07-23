param(
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$testResultsRoot = Join-Path $repoRoot 'TestResults'
$reportsRoot = Join-Path $repoRoot 'CoverageReports'
$toolsPath = Join-Path $repoRoot '.tools'
$reportGeneratorExe = Join-Path $toolsPath 'reportgenerator.exe'

$unitProject = Join-Path $repoRoot 'tests/Biak.ConsoleApp.UnitTests/Biak.ConsoleApp.UnitTests.csproj'
$integrationProject = Join-Path $repoRoot 'tests/Biak.ConsoleApp.IntegrationTests/Biak.ConsoleApp.IntegrationTests.csproj'

function Get-CoverageFilePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Directory
    )

    $coverageFile = Get-ChildItem -Path $Directory -Recurse -Filter 'coverage.cobertura.xml' |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $coverageFile) {
        throw "Coverage file not found under '$Directory'."
    }

    return $coverageFile.FullName
}

Push-Location $repoRoot
try {
    Write-Host 'Cleaning previous coverage artifacts...'
    Remove-Item -Path $testResultsRoot -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path $reportsRoot -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host 'Ensuring ReportGenerator tool is available...'
    New-Item -ItemType Directory -Path $toolsPath -Force | Out-Null
    if (Test-Path $reportGeneratorExe) {
        dotnet tool update dotnet-reportgenerator-globaltool --tool-path $toolsPath | Out-Null
    }
    else {
        dotnet tool install dotnet-reportgenerator-globaltool --tool-path $toolsPath | Out-Null
    }

    if ($NoBuild) {
        Write-Host 'Running unit tests with coverage (no build)...'
        dotnet test $unitProject --no-build --collect:"XPlat Code Coverage;Format=cobertura" --results-directory "$testResultsRoot/Unit"

        Write-Host 'Running integration tests with coverage (no build)...'
        dotnet test $integrationProject --no-build --collect:"XPlat Code Coverage;Format=cobertura" --results-directory "$testResultsRoot/Integration"
    }
    else {
        Write-Host 'Running unit tests with coverage...'
        dotnet test $unitProject --collect:"XPlat Code Coverage;Format=cobertura" --results-directory "$testResultsRoot/Unit"

        Write-Host 'Running integration tests with coverage...'
        dotnet test $integrationProject --collect:"XPlat Code Coverage;Format=cobertura" --results-directory "$testResultsRoot/Integration"
    }

    $unitCoverageFile = Get-CoverageFilePath -Directory (Join-Path $testResultsRoot 'Unit')
    $integrationCoverageFile = Get-CoverageFilePath -Directory (Join-Path $testResultsRoot 'Integration')

    $unitReportDir = Join-Path $reportsRoot 'unit'
    $integrationReportDir = Join-Path $reportsRoot 'integration'

    Write-Host 'Generating unit HTML coverage report...'
    & $reportGeneratorExe "-reports:$unitCoverageFile" "-targetdir:$unitReportDir" '-reporttypes:Html'

    Write-Host 'Generating integration HTML coverage report...'
    & $reportGeneratorExe "-reports:$integrationCoverageFile" "-targetdir:$integrationReportDir" '-reporttypes:Html'

    Write-Host ''
    Write-Host 'Done.'
    Write-Host "Unit report: $unitReportDir/index.html"
    Write-Host "Integration report: $integrationReportDir/index.html"
}
finally {
    Pop-Location
}

[CmdletBinding()]
param(
    [ValidateRange(0, 100)]
    [double] $MinimumLineCoverage = 75,
    [switch] $NoRestore
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'SupportAssistant.UnitTests.csproj'
$results = Join-Path $PSScriptRoot ('TestResults/verified-' + [guid]::NewGuid().ToString('N'))
$arguments = @('test', $project, '--collect:XPlat Code Coverage', '--results-directory', $results,
    '--logger', 'trx', '--logger', 'console;verbosity=minimal')
if ($NoRestore) { $arguments += '--no-restore' }

& dotnet @arguments
if ($LASTEXITCODE -ne 0) { throw "Unit tests failed (exit code $LASTEXITCODE)." }

$reports = @(Get-ChildItem -LiteralPath $results -Filter 'coverage.cobertura.xml' -Recurse)
if ($reports.Count -eq 0) { throw 'No coverage report was produced.' }
# The TRX logger can copy the collector attachment into its own results folder.
# Accept identical copies, but never silently select one of different reports.
$hashes = @($reports | Get-FileHash -Algorithm SHA256 | Select-Object -ExpandProperty Hash -Unique)
if ($hashes.Count -ne 1) { throw "Expected one distinct coverage report; found $($hashes.Count)." }
[xml] $report = Get-Content -LiteralPath $reports[0].FullName -Raw
$coverage = $report.coverage
$names = @($coverage.packages.package | ForEach-Object { $_.name })
foreach ($assembly in @('SupportAssistant.Api', 'SupportAssistant.Application', 'SupportAssistant.Infrastructure')) {
    if ($assembly -notin $names) { throw "Coverage report is missing production assembly $assembly." }
}
if ([int] $coverage.'lines-valid' -eq 0) { throw 'Coverage report contains no executable lines.' }

$linePercent = 100.0 * [int] $coverage.'lines-covered' / [int] $coverage.'lines-valid'
$branchPercent = if ([int] $coverage.'branches-valid' -gt 0) {
    100.0 * [int] $coverage.'branches-covered' / [int] $coverage.'branches-valid'
} else { 100.0 }

Write-Host ('Line coverage: {0:F2}% ({1}/{2}); branch coverage: {3:F2}% ({4}/{5})' -f
    $linePercent, $coverage.'lines-covered', $coverage.'lines-valid',
    $branchPercent, $coverage.'branches-covered', $coverage.'branches-valid')
Write-Host "Report: $($reports[0].FullName)"

if ($linePercent -lt $MinimumLineCoverage) {
    throw ('Line coverage {0:F2}% is below required {1:F2}%.' -f $linePercent, $MinimumLineCoverage)
}

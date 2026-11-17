#Requires -Version 7.0
<#
.SYNOPSIS
    Fails the build when line coverage falls below the agreed minimum.

.DESCRIPTION
    Reads a Cobertura report produced by ReportGenerator and checks the overall line
    rate and every assembly's line rate against -Minimum.

    Both are checked on purpose. An overall figure alone can stay green while one
    assembly rots: measured on 12.11.2026 the project stood at 93.9 % while the rules
    engine, which decides what counts as a finding, sat at 68.5 %. The per-assembly
    check is what stops an average from hiding that.

    ReportGenerator's merged report is the input rather than the raw collector output.
    Each test project emits its own file covering overlapping assemblies, and a line is
    covered if any project reaches it. Merging unions those hits; reading one file per
    assembly undercounts - on the same run that difference was 14 points for
    AdoExport.Auditing.

.PARAMETER ReportPath
    The merged Cobertura.xml.

.PARAMETER Minimum
    The required line rate, in percent, for the total and for every assembly.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ReportPath,

    [double] $Minimum = 90.0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ReportPath)) {
    throw "No coverage report at $ReportPath. The merge step did not run, or produced nothing."
}

[xml] $report = Get-Content -LiteralPath $ReportPath -Raw

# Explicit XML methods rather than dot notation: the Cobertura DOCTYPE puts a
# DocumentType node beside the root, and $report.coverage does not resolve past it.
$root = $report.DocumentElement

$overall = [math]::Round([double] $root.GetAttribute('line-rate') * 100, 2)

$assemblies = foreach ($package in $root.SelectNodes('packages/package')) {
    [pscustomobject]@{
        Name = $package.GetAttribute('name')
        Rate = [math]::Round([double] $package.GetAttribute('line-rate') * 100, 2)
    }
}

if ($assemblies.Count -eq 0) {
    throw "The coverage report at $ReportPath lists no assemblies. Something upstream produced an empty report."
}

'Line coverage'
'-------------'
foreach ($assembly in ($assemblies | Sort-Object Rate)) {
    $mark = if ($assembly.Rate -lt $Minimum) { 'FAIL' } else { 'ok  ' }
    '{0}  {1,-28} {2,6:N1} %' -f $mark, $assembly.Name, $assembly.Rate
}

$totalMark = if ($overall -lt $Minimum) { 'FAIL' } else { 'ok  ' }
'{0}  {1,-28} {2,6:N1} %' -f $totalMark, 'TOTAL', $overall
''

$failures = [System.Collections.Generic.List[object]]::new()

if ($overall -lt $Minimum) {
    $failures.Add([pscustomobject]@{ Name = 'TOTAL'; Rate = $overall })
}

foreach ($assembly in $assemblies) {
    if ($assembly.Rate -lt $Minimum) {
        $failures.Add($assembly)
    }
}

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) {
        # logissue puts the reason on the build summary instead of only in the log.
        Write-Host "##vso[task.logissue type=error]$($failure.Name) is at $($failure.Rate) %, below the required $Minimum %."
    }

    exit 1
}

"All assemblies and the total are at or above $Minimum %."

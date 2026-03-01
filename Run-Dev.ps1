#requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$csproj = Join-Path $here 'WorkstationV2\WorkstationV2.csproj'

if (-not (Test-Path -LiteralPath $csproj)) {
    $alt = Get-ChildItem -LiteralPath $here -Recurse -Filter '*.csproj' -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -ieq 'WorkstationV2.csproj' } |
        Select-Object -First 1
    if ($alt) { $csproj = $alt.FullName }
}

if (-not (Test-Path -LiteralPath $csproj)) {
    throw "WorkstationV2.csproj not found under: $here"
}

& dotnet run --project $csproj --configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw "dotnet run failed ($LASTEXITCODE)" }
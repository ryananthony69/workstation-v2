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
$code = $LASTEXITCODE

if ($code -ne 0) {
    # Common unhandled exception exit code from .NET: 0xE0434352 == -532462766
    if ($code -eq -532462766) {
        $log = Join-Path $env:LOCALAPPDATA 'WorkstationV2\crash.log'
        Write-Host "App exited with unhandled exception code ($code). Check crash log: $log"
    }
    throw "dotnet run failed ($code)"
}
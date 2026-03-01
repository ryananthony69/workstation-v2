# Handoff

## What changed
- Added Run-Dev.ps1 so you can run the app from any PowerShell working directory using an absolute project path
- Updated README.md (if present) to mention Run-Dev.ps1
- Maintained required .gitignore secret exclusions

## How to run/use
- From ANY PowerShell path:
  - & "C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\Run-Dev.ps1"
- Or explicitly:
  - dotnet run --project "C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\WorkstationV2\WorkstationV2.csproj"

## How to test/verify
- Open PowerShell in any folder (e.g., C:\Windows\System32)
- Run:
  - & "C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\Run-Dev.ps1"
- App should build + launch without the MSB1009 path error.

## Known issues
- None specific; previous error was caused by running dotnet with a relative project path from the wrong working directory.

## Next steps
- Add Run-Build.ps1 (optional) for dotnet build / dotnet publish shortcuts
- Add a Start-Workstation.ps1 launcher that opens the app and optionally primes Tools/State paths

## Next ChatGPT Prompt
We hit MSB1009 because dotnet run was executed from a directory that didn't contain WorkstationV2\WorkstationV2.csproj.
We added Run-Dev.ps1 at repo root to run dotnet using the correct project path from anywhere.
Next, add a Run-Publish.ps1 that publishes a Release build to .\dist\ and documents how to run the published exe.
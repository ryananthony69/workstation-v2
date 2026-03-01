# Handoff

## What changed
- Fixed build failures by making the project WPF-only (no WinForms) to eliminate ambiguous type errors (Button/UserControl/Application/KeyEventArgs).
- Reworked Script Runner to execute scripts via powershell.exe (stdin) and capture stdout/stderr; removed System.Management.Automation dependency that caused missing namespace errors.
- Reworked CanvasPanel to avoid WinForms folder picker; uses WPF-only OpenFileDialog folder-selection workaround.
- Corrected build/run commands:
  - dotnet build does NOT support --project; use dotnet build <path>.

## How to run/use
- From repo root:
  - dotnet build ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\WorkstationV2\WorkstationV2.csproj"" -c Debug
  - dotnet run --project ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\WorkstationV2\WorkstationV2.csproj""
- From ANY PowerShell directory:
  - & ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\Run-Dev.ps1""

## How to test/verify
- Build:
  - dotnet build ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\WorkstationV2\WorkstationV2.csproj"" -c Debug
- Run:
  - & ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\Run-Dev.ps1""
- Verify:
  - App launches
  - Script Runner: enter Get-Date then Ctrl+Enter; output shows and is copied
  - Canvas Panel: Browse Vault / Pick Canvas works; Open Canvas triggers Obsidian link

## Known issues
- Vault folder picker uses a WPF OpenFileDialog workaround (select folder by navigating to it and clicking Open).

## Next steps
- Add Run-Publish.ps1 to publish Release output into .\dist\ and document running the published exe.
- Re-add in-process PowerShell later only if needed (without enabling WinForms).

## Next ChatGPT Prompt
The build was failing due to WinForms/WPF type ambiguities and missing System.Management.Automation.
We fixed the project to be WPF-only and changed Script Runner to execute scripts via powershell.exe.
Next: add a Run-Publish.ps1 to publish Release to .\dist\ and update README/Handoff with how to run the published exe.
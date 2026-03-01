# Handoff

## What changed
- Fixed likely startup crash by removing WebView2 control from XAML:
  - BrowserTile now creates WebView2 dynamically in code with try/catch
  - If WebView2 fails to initialize, the tile shows a fallback message instead of crashing the app
- Added crash logging + unhandled exception handling:
  - Writes %LOCALAPPDATA%\WorkstationV2\crash.log
  - Shows a message box for unhandled UI-thread exceptions (and logs them)
- Updated Run-Dev.ps1 to point to crash.log when the process exits with the common .NET unhandled exception code (-532462766)

## How to run/use
- From ANY PowerShell directory:
  - & ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\Run-Dev.ps1""
- If the app exits unexpectedly:
  - Check: %LOCALAPPDATA%\WorkstationV2\crash.log

## How to test/verify
- Build:
  - dotnet build ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\WorkstationV2\WorkstationV2.csproj"" -c Debug
- Run:
  - & ""C:\Users\Ryana_pzq4frx\Desktop\workstation-v2\Run-Dev.ps1""
- Verify:
  - App launches
  - If WebView2 is unavailable, browser tiles show a fallback message (app stays open)
  - If an exception occurs, crash.log records it

## Known issues
- Browser tiles require WebView2 runtime for full browsing; without it, fallback text is shown.

## Next steps
- Add a small “Copy crash log path” button in the UI when a browser tile falls back
- Add a non-modal status banner for browser initialization failures

## Next ChatGPT Prompt
We fixed a runtime crash by removing WebView2 from XAML and instantiating it dynamically with try/catch + fallback UI, and added crash logging to %LOCALAPPDATA%\WorkstationV2\crash.log.
Next: implement Run-Publish.ps1 to publish Release to .\dist\ and update README/Handoff with how to run the published exe.
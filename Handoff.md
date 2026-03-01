# Handoff

## What changed
- Created a new WPF (.NET 8) project scaffold for “Workstation v2”
- Added 3 WebView2 browser tiles with URL bars and basic navigation persistence
- Added Obsidian Canvas control panel tile (deep-link open + vault open + focus)
- Added right sidebar with config-driven tool buttons (Tools.json)
- Added Script Runner (PowerShell via Microsoft.PowerShell.SDK) with Ctrl+Enter and auto-copy output
- Added state/config persistence seeded on first run:
  - Tools: %LOCALAPPDATA%\WorkstationV2\Tools.json
  - State: %LOCALAPPDATA%\WorkstationV2\WorkstationState.json
- Added .gitignore with required secret exclusions

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Edit tool buttons at: %LOCALAPPDATA%\WorkstationV2\Tools.json
- URLs and Canvas settings persist at: %LOCALAPPDATA%\WorkstationV2\WorkstationState.json

## How to test/verify
- Launch and verify:
  - All 3 tiles navigate and update URL bars
  - Closing/reopening restores Tile1/2/3 URLs
  - Canvas panel selects a vault folder and a .canvas file
  - Open Canvas triggers Obsidian to open by file path
  - Script Runner executes Get-Date and copies output to clipboard
  - Tools panel buttons perform configured actions

## Known issues
- If WebView2 initialization fails on a machine, tiles may appear blank (no crash expected).
- Obsidian focus is best-effort (depends on process/window availability).
- Tools.json validation is minimal; malformed config may cause some tools to no-op.

## Next steps
- Add splitter support (resizable sidebar/tiles) and persist splitter distances
- Add hotkeys for focusing tiles and running tools
- Expand tool types (external browser open, launch app with working dir)
- Add optional experimental embed mode for Obsidian if still desired

## Next ChatGPT Prompt
We have a repo scaffolded for Workstation v2 (WPF .NET 8) with:
- 3 WebView2 tiles (BrowserTile control), state persistence in %LOCALAPPDATA%\WorkstationV2\WorkstationState.json
- CanvasPanel control that opens Obsidian via obsidian://open?path=
- Tools.json config-driven tool buttons (right sidebar)
- ScriptRunner control that runs PowerShell and auto-copies output

Next, implement a resizable layout:
1) Add a GridSplitter between left area and right sidebar, and persist sidebar width.
2) Add splitters inside the left 2x2 area (or redesign to a DockPanel with splitters) and persist row/column sizes.
3) Update AppState to store these sizes and restore on startup.
Also update Handoff.md with what changed and how to test.
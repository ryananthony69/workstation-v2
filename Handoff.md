# Handoff

## What changed
- Added keyboard shortcuts:
  - Ctrl+1 / Ctrl+2 / Ctrl+3 focuses Tile1 / Tile2 / Tile3
  - Ctrl+Shift+1..9 runs tool buttons 1..9 from Tools.json order
- Added on-screen toast/status overlay in the right sidebar when tools run or shortcuts trigger
- Ignored and removed WPF temp project files (*_wpftmp.csproj) from the repo

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Keyboard shortcuts:
  - Ctrl+1 / Ctrl+2 / Ctrl+3 = focus browser tiles
  - Ctrl+Shift+1..9 = run first 9 tools from Tools.json
- Tools config:
  - %LOCALAPPDATA%\WorkstationV2\Tools.json

## How to test/verify
- Launch the app
- Press Ctrl+1 / Ctrl+2 / Ctrl+3 and confirm focus switches between tiles
- Press Ctrl+Shift+1..9 and confirm:
  - Toast appears indicating the tool
  - The tool action runs (e.g., OpenUrl navigates a tile, RunScript fills Script Runner and runs)
- Confirm *_wpftmp.csproj is not present in the repo and does not reappear after running/building

## Known issues
- Some key combos may be swallowed by embedded browser content in edge cases; PreviewKeyDown on the Window handles most cases.
- Tools.json validation is minimal; malformed config may cause some tools to no-op.

## Next steps
- Add UI labels/hints for shortcuts (e.g., “Tool 1” badge) and an optional shortcuts cheat-sheet panel
- Add robust Tools.json validation with a visible error banner and a “Reload Tools” button
- Add tool types:
  - OpenUrlExternal (default browser)
  - LaunchApp with working directory
  - OpenFile / OpenFolder
- Add a lightweight “tool search” box to filter the 12 buttons

## Next ChatGPT Prompt
We added keyboard shortcuts and toast status in Workstation v2:
- Ctrl+1/2/3 focuses Tile1/2/3
- Ctrl+Shift+1..9 runs tool buttons 1..9 from Tools.json order
- Toast overlay shows tool/shortcut status
- Ignored/removed *_wpftmp.csproj temp files

Next, implement tool reload + validation:
1) Add a “Reload Tools” button above the tools grid.
2) On reload, re-read %LOCALAPPDATA%\WorkstationV2\Tools.json and rebuild the grid.
3) Validate the schema (label/type required) and show a visible error banner/toast for invalid entries.
Update Handoff.md accordingly.
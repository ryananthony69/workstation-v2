# Handoff

## What changed
- Added resizable splitters:
  - Between left area and right sidebar
  - Between left grid columns (Tile1/Tile3 vs Tile2/Canvas)
  - Between left grid rows (top tiles vs bottom tiles)
- Persisted layout sizing to state:
  - SidebarWidth (pixels)
  - LeftColumnRatio (top-left column ratio)
  - LeftRowRatio (top row ratio)
- Updated default seeded state (DefaultWorkstationState.json) to include layout fields

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Drag splitters to resize:
  - Sidebar width
  - Left tiles grid column divider
  - Left tiles grid row divider
- State persists to:
  - %LOCALAPPDATA%\WorkstationV2\WorkstationState.json

## How to test/verify
- Launch app
- Drag each splitter to new positions
- Close app
- Relaunch app and confirm:
  - Sidebar width is restored
  - Left grid row/column split positions are restored
  - URLs and Canvas paths still persist as before

## Known issues
- If WebView2 initialization fails on a machine, tiles may appear blank (no crash expected).
- Obsidian focus is best-effort (depends on process/window availability).
- Tools.json validation is minimal; malformed config may cause some tools to no-op.

## Next steps
- Add hotkeys for focusing tiles and triggering tool buttons
- Add per-tile “Open in external browser” tool type (config-driven)
- Add minimal config validation + UI error banner for bad Tools.json
- Add optional “experimental embed” mode for Obsidian if still desired

## Next ChatGPT Prompt
We now have splitters and layout persistence in Workstation v2:
- Sidebar splitter (persist SidebarWidth)
- Left grid row/column splitters (persist LeftRowRatio / LeftColumnRatio)
- State saved in %LOCALAPPDATA%\WorkstationV2\WorkstationState.json

Next, add keyboard shortcuts:
1) Ctrl+1 / Ctrl+2 / Ctrl+3 focuses Tile1/Tile2/Tile3 WebView.
2) Ctrl+Shift+1..9 triggers the first 9 tool buttons from Tools.json.
3) Add a small on-screen toast/status text when a tool runs.
Update Handoff.md accordingly.
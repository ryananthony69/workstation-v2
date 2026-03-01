# Handoff

## What changed
- Added a “Reload Tools” button above the tools grid
- Tools can now be reloaded from:
  - %LOCALAPPDATA%\WorkstationV2\Tools.json
- Added validation for Tools.json entries:
  - Requires label + type
  - Validates minimal required fields for known types (e.g., OpenUrl requires payload.url)
- Added a visible error banner in the Tools panel when Tools.json has invalid entries
- Improved tool button rendering:
  - Invalid tools are disabled with tooltip showing the reason
- Kept existing splitters + layout persistence + keyboard shortcuts + toast overlay

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Edit tools:
  - %LOCALAPPDATA%\WorkstationV2\Tools.json
- Click “Reload Tools” to apply changes without restarting
- Shortcuts:
  - Ctrl+1 / Ctrl+2 / Ctrl+3 = focus Tile1 / Tile2 / Tile3
  - Ctrl+Shift+1..9 = run tools #1..#9 in Tools.json order

## How to test/verify
- Launch app and confirm Tools panel shows “Reload Tools”
- Edit %LOCALAPPDATA%\WorkstationV2\Tools.json:
  - Add a valid tool; click Reload Tools; verify it appears and works
  - Break a tool (e.g., remove label/type, or remove payload.url for OpenUrl); click Reload Tools:
    - Error banner appears describing issues
    - Invalid tool buttons are disabled
- Confirm shortcuts still work (Ctrl+1/2/3 and Ctrl+Shift+1..9)

## Known issues
- If Tools.json is malformed JSON, reload shows an error banner and keeps prior tools in memory.
- Unknown tool types are treated as invalid until implemented.

## Next steps
- Add a “Reload State” (WorkstationState.json) button for faster iteration
- Add an “Edit Tools.json” convenience button (opens file in default editor)
- Add a small “tools search/filter” textbox above the grid
- Add schema versioning for Tools.json and migration behavior

## Next ChatGPT Prompt
We now support reloading and validating Tools.json in Workstation v2:
- Reload Tools button reloads %LOCALAPPDATA%\WorkstationV2\Tools.json
- Invalid entries show an error banner; invalid buttons are disabled with tooltip
- Existing features still present: WebView2 tiles, Canvas panel, ScriptRunner, splitters + persisted layout, shortcuts, toast

Next, implement a quick edit workflow:
1) Add a button “Open Tools.json” next to “Reload Tools” that opens the file in the default editor.
2) Add “Export Tools.json” button that copies the current in-memory tools config to clipboard as JSON.
3) Add “Import Tools.json” textbox/modal that allows pasting JSON and saving it to %LOCALAPPDATA%\WorkstationV2\Tools.json with validation.
Update Handoff.md accordingly.
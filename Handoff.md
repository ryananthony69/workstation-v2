# Handoff

## What changed
- Added Tools quick-edit workflow:
  - Open Tools.json (opens in Notepad)
  - Export (Clipboard): copies current in-memory tools config as JSON
  - Import: opens a modal editor to paste JSON, validates, then saves to %LOCALAPPDATA%\WorkstationV2\Tools.json
- Import validates Tools.json schema and prevents saving invalid entries
- Reload still supported (re-reads Tools.json on disk)
- Kept existing features: WebView2 tiles, Canvas panel, ScriptRunner, splitters + persisted layout, shortcuts, toast, tools validation banner

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Tools config file:
  - %LOCALAPPDATA%\WorkstationV2\Tools.json
- Tools panel buttons:
  - Reload = reload from disk
  - Open Tools.json = opens in Notepad
  - Export (Clipboard) = copies current tools JSON to clipboard
  - Import... = paste tools JSON, validate, and save to disk
- Shortcuts:
  - Ctrl+1 / Ctrl+2 / Ctrl+3 = focus Tile1 / Tile2 / Tile3
  - Ctrl+Shift+1..9 = run tools #1..#9 in Tools.json order

## How to test/verify
- Launch app and confirm Tools panel shows: Reload, Open Tools.json, Export (Clipboard), Import...
- Export:
  - Click Export (Clipboard), paste into a text editor, confirm valid JSON with root "tools" array
- Import (valid):
  - Click Import..., paste valid JSON, click Validate + Save
  - Confirm tools grid updates immediately and Tools.json on disk is updated
- Import (invalid):
  - Remove a required field (label/type or payload.url for OpenUrl) and try saving
  - Confirm the dialog shows validation errors and does not save
- Open:
  - Click Open Tools.json and confirm it opens in Notepad

## Known issues
- Import dialog uses a basic modal editor (no syntax highlighting).
- If Tools.json is malformed JSON, reload/import show errors and preserve last in-memory tools.

## Next steps
- Add tools search/filter box to quickly find buttons
- Add “Open Tools folder” convenience button
- Add “Export Selected Tool” / “Duplicate Tool” UI actions
- Add optional schema versioning and migrations for Tools.json

## Next ChatGPT Prompt
We added a Tools quick-edit workflow to Workstation v2:
- Open Tools.json (Notepad)
- Export (Clipboard) to copy current ToolsConfig as JSON
- Import modal to paste JSON, validate it, and save to %LOCALAPPDATA%\WorkstationV2\Tools.json
- Reload Tools still supported with validation banner and disabled invalid buttons

Next, implement tools search + quick run:
1) Add a search textbox above the tools grid to filter by label (case-insensitive).
2) Show only matching tools in the grid and preserve ordering.
3) Add Enter key behavior: if search narrows to exactly 1 tool, Enter runs it.
Update Handoff.md accordingly.
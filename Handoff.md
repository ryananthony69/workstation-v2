# Handoff

## What changed
- Added a compact shortcut hint line under the Tools search box:
  - Ctrl+F focus, Esc clear, Up/Down select, Enter run, Tab to tools
- Added a “Show shortcut hints” checkbox to show/hide the hint
- Persisted the checkbox value in WorkstationState.json as ToolsHintVisible (nullable migration; defaults to true if missing)
- Updated DefaultWorkstationState.json to seed ToolsHintVisible=true

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Tools panel:
  - Use “Show shortcut hints” checkbox to show/hide the hint line
  - The preference persists to: %LOCALAPPDATA%\WorkstationV2\WorkstationState.json

## How to test/verify
- Launch app
- Toggle “Show shortcut hints” off:
  - Hint line disappears
- Close app and relaunch:
  - Hint visibility remains as last set
- Delete ToolsHintVisible from %LOCALAPPDATA%\WorkstationV2\WorkstationState.json and relaunch:
  - Hint defaults to visible (migration behavior)

## Known issues
- None specific; this is a UI-only addition.

## Next steps
- Add a small “?” icon that shows a short tooltip/cheatsheet (all shortcuts) without taking layout space
- Add an option to hide the Tools error banner when there are only minor validation warnings

## Next ChatGPT Prompt
We added a Tools shortcut hint line under the search box with a “Show shortcut hints” checkbox persisted to WorkstationState.json (ToolsHintVisible).
Next, add a tooltip cheatsheet:
1) Add a small “?” button next to the checkbox.
2) Clicking it shows a modal or popup listing all shortcuts (Ctrl+F, Esc, Tab, Up/Down/Enter, Ctrl+1/2/3, Ctrl+Shift+1..9).
3) Persist whether the cheatsheet should show on first launch after updates (one-time).
Update Handoff.md accordingly.
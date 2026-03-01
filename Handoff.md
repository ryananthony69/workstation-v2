# Handoff

## What changed
- Added “global tools hotkey mode” behavior:
  - Ctrl+F focuses the Tools search box (selects text)
  - Esc clears the Tools search box and resets tool list to all tools
  - When Tools search has focus, Tab moves focus into the tools grid (first selected tool button)
- Kept selection navigation active even when focus is on tool buttons:
  - Up/Down changes selection and moves focus to the selected tool
  - Enter runs the selected tool
- No changes to Tools.json schema or persistence paths

## How to run/use
- dotnet run --project .\WorkstationV2\WorkstationV2.csproj
- Tools workflow:
  - Ctrl+F = focus Tools search
  - Type to filter tools by label
  - Up/Down = change selection (works in search box and on tool buttons)
  - Enter = run selected tool (or the only match)
  - Tab (from search box) = move focus into tool buttons
  - Esc = clear search and reset to all tools
- Existing shortcuts remain:
  - Ctrl+1/2/3 = focus tiles
  - Ctrl+Shift+1..9 = run tools #1..#9 (Tools.json order)

## How to test/verify
- Launch app
- Press Ctrl+F and confirm search box is focused and selected
- Type a search query, then:
  - Press Tab: focus moves to a tool button and selection stays highlighted
  - Press Up/Down: selection moves and focus follows
  - Press Enter: selected tool runs
- Press Esc:
  - Search clears and all tools are shown again
  - Search box is focused

## Known issues
- If a WebView2 tile is capturing certain key combos in rare cases, PreviewKeyDown usually still handles Ctrl+F/Esc but behavior may vary by focus.

## Next steps
- Add a small UI hint line under the search box: “Ctrl+F focus, Esc clear, Up/Down select, Enter run”
- Add optional “focus tools panel” command that also scrolls tools into view if needed

## Next ChatGPT Prompt
We added global tools hotkeys in Workstation v2:
- Ctrl+F focuses Tools search
- Esc clears search and resets tool list
- Tab from search moves focus into the tools grid
- Up/Down/Enter selection navigation works even when focus is on tool buttons

Next, add a compact shortcut hint UI:
1) Add a small hint text under the Tools search box showing the hotkeys.
2) Add a toggle (checkbox) to show/hide the hint; persist it in WorkstationState.json.
Update Handoff.md accordingly.
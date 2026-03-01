# Workstation v2 (WPF + WebView2)

Single-window Windows dashboard:
- 3 browser tiles (WebView2)
- Canvas control panel for Obsidian (deep-link/open actions; no embedding by default)
- Right sidebar with config-driven tool buttons (Tools.json)
- Script Runner (paste PowerShell → run → output + auto-copy)

## Run (dev)
- `dotnet run --project .\WorkstationV2\WorkstationV2.csproj`

## Config + state location (runtime)
The app stores user-editable config/state under:
- `%LOCALAPPDATA%\WorkstationV2\Tools.json`
- `%LOCALAPPDATA%\WorkstationV2\WorkstationState.json`

On first run, the app seeds these from the files shipped with the app:
- `Tools.json`
- `DefaultWorkstationState.json`
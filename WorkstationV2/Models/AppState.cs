namespace WorkstationV2.Models;

public class AppState
{
    public string? Tile1Url { get; set; }
    public string? Tile2Url { get; set; }
    public string? Tile3Url { get; set; }

    public string? VaultPath { get; set; }
    public string? CanvasPath { get; set; }

    public int LastFocusedTile { get; set; } = 1;

    public int WindowWidth { get; set; } = 1600;
    public int WindowHeight { get; set; } = 900;

    // Layout persistence
    public double SidebarWidth { get; set; } = 380;
    public double LeftColumnRatio { get; set; } = 0.5; // left column / (left + right)
    public double LeftRowRatio { get; set; } = 0.5;    // top row / (top + bottom)

    // UI preferences
    // Nullable for migration: if missing in older state files, treat as true.
    public bool? ToolsHintVisible { get; set; } = null;
}
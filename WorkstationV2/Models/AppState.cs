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
}
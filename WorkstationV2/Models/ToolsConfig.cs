using System.Collections.Generic;

namespace WorkstationV2.Models;

public class ToolsConfig
{
    public List<ToolItem>? Tools { get; set; }
}

public class ToolItem
{
    public string? Label { get; set; }
    public string? Type { get; set; }
    public int? TileTarget { get; set; }
    public Dictionary<string, string>? Payload { get; set; }
}
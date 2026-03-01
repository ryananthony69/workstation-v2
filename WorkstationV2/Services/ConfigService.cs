using System;
using System.IO;
using System.Text;
using System.Text.Json;
using WorkstationV2.Models;

namespace WorkstationV2.Services;

public class ConfigService
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true
    };

    private static string AppDataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkstationV2");

    private static string StatePath => Path.Combine(AppDataDir, "WorkstationState.json");
    private static string ToolsPath => Path.Combine(AppDataDir, "Tools.json");

    private static string SeedStatePath => Path.Combine(AppContext.BaseDirectory, "DefaultWorkstationState.json");
    private static string SeedToolsPath => Path.Combine(AppContext.BaseDirectory, "Tools.json");

    public AppState LoadStateSeedIfMissing()
    {
        Directory.CreateDirectory(AppDataDir);

        if (!File.Exists(StatePath))
        {
            if (File.Exists(SeedStatePath))
            {
                File.Copy(SeedStatePath, StatePath, overwrite: true);
            }
            else
            {
                SaveState(new AppState());
            }
        }

        return Load<AppState>(StatePath) ?? new AppState();
    }

    public ToolsConfig LoadToolsSeedIfMissing()
    {
        Directory.CreateDirectory(AppDataDir);

        if (!File.Exists(ToolsPath))
        {
            if (File.Exists(SeedToolsPath))
            {
                File.Copy(SeedToolsPath, ToolsPath, overwrite: true);
            }
            else
            {
                SaveTools(new ToolsConfig());
            }
        }

        return Load<ToolsConfig>(ToolsPath) ?? new ToolsConfig();
    }

    public void SaveState(AppState state) => Save(StatePath, state);
    public void SaveTools(ToolsConfig cfg) => Save(ToolsPath, cfg);

    private static T? Load<T>(string path)
    {
        try
        {
            var json = File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }
        catch
        {
            return default;
        }
    }

    private static void Save<T>(string path, T obj)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(obj, JsonOpts);
        File.WriteAllText(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
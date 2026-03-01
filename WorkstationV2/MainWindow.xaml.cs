using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using WorkstationV2.Models;
using WorkstationV2.Services;

namespace WorkstationV2;

public partial class MainWindow : Window
{
    private readonly ConfigService _config = new();
    private AppState _state = new();
    private ToolsConfig _tools = new();

    private CancellationTokenSource? _saveCts;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _state = _config.LoadStateSeedIfMissing();
        _tools = _config.LoadToolsSeedIfMissing();

        ApplyLayoutFromState();

        Tile1.Initialize(_state.Tile1Url ?? string.Empty, url => OnTileUrlChanged(1, url), () => SetLastFocused(1));
        Tile2.Initialize(_state.Tile2Url ?? string.Empty, url => OnTileUrlChanged(2, url), () => SetLastFocused(2));
        Tile3.Initialize(_state.Tile3Url ?? string.Empty, url => OnTileUrlChanged(3, url), () => SetLastFocused(3));

        CanvasPanel.Initialize(
            _state.VaultPath ?? string.Empty,
            _state.CanvasPath ?? string.Empty,
            (vault, canvas) =>
            {
                _state.VaultPath = vault;
                _state.CanvasPath = canvas;
                ScheduleSave();
            },
            () => ScheduleSave()
        );

        BuildToolsGrid();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveNow();
    }

    private void BuildToolsGrid()
    {
        ToolsGrid.Children.Clear();
        var list = _tools.Tools ?? new List<ToolItem>();
        foreach (var tool in list.Take(120))
        {
            var btn = new Button
            {
                Content = tool.Label ?? "(tool)",
                Margin = new Thickness(4),
                Padding = new Thickness(8),
                Tag = tool
            };
            btn.Click += ToolButton_Click;
            ToolsGrid.Children.Add(btn);
        }
    }

    private void ToolButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b) return;
        if (b.Tag is not ToolItem tool) return;
        ExecuteTool(tool);
    }

    private void ExecuteTool(ToolItem tool)
    {
        var type = (tool.Type ?? string.Empty).Trim();

        try
        {
            switch (type)
            {
                case "OpenUrl":
                {
                    var url = tool.Payload?.GetValueOrDefault("url") ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(url)) return;

                    var target = tool.TileTarget ?? 1;
                    NavigateTile(target, url);
                    break;
                }
                case "FocusTile":
                {
                    var target = tool.TileTarget ?? 1;
                    FocusTile(target);
                    break;
                }
                case "LaunchApp":
                {
                    var exe = tool.Payload?.GetValueOrDefault("exe") ?? string.Empty;
                    var args = tool.Payload?.GetValueOrDefault("args") ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(exe)) return;

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exe,
                        Arguments = args,
                        UseShellExecute = true
                    });
                    break;
                }
                case "RunScript":
                {
                    var script = tool.Payload?.GetValueOrDefault("script") ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(script)) return;

                    ScriptRunner.SetInput(script);
                    ScriptRunner.RunCurrent();
                    break;
                }
                case "OpenCanvas":
                {
                    var mode = tool.Payload?.GetValueOrDefault("mode") ?? "state";
                    var path = mode == "state" ? (_state.CanvasPath ?? string.Empty) : (tool.Payload?.GetValueOrDefault("path") ?? string.Empty);
                    CanvasPanel.OpenCanvas(path);
                    break;
                }
                case "OpenVault":
                {
                    var mode = tool.Payload?.GetValueOrDefault("mode") ?? "state";
                    var path = mode == "state" ? (_state.VaultPath ?? string.Empty) : (tool.Payload?.GetValueOrDefault("path") ?? string.Empty);
                    CanvasPanel.OpenVault(path);
                    break;
                }
                case "FocusObsidian":
                {
                    WindowActivator.FocusProcessMainWindow("Obsidian");
                    break;
                }
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            ScriptRunner.SetOutput($"Tool error: {ex.Message}");
        }
    }

    private void NavigateTile(int tileId, string url)
    {
        switch (tileId)
        {
            case 2: Tile2.Navigate(url); break;
            case 3: Tile3.Navigate(url); break;
            default: Tile1.Navigate(url); break;
        }
    }

    private void FocusTile(int tileId)
    {
        switch (tileId)
        {
            case 2: Tile2.FocusWeb(); break;
            case 3: Tile3.FocusWeb(); break;
            default: Tile1.FocusWeb(); break;
        }
        SetLastFocused(tileId);
    }

    private void OnTileUrlChanged(int tileId, string url)
    {
        switch (tileId)
        {
            case 2: _state.Tile2Url = url; break;
            case 3: _state.Tile3Url = url; break;
            default: _state.Tile1Url = url; break;
        }
        ScheduleSave();
    }

    private void SetLastFocused(int tileId)
    {
        _state.LastFocusedTile = tileId;
        ScheduleSave();
    }

    private static double Clamp(double v, double min, double max)
        => v < min ? min : (v > max ? max : v);

    private void ApplyLayoutFromState()
    {
        // Sidebar pixel width
        var sidebar = _state.SidebarWidth <= 0 ? 380 : _state.SidebarWidth;
        sidebar = Clamp(sidebar, 280, 1200);
        SidebarCol.Width = new GridLength(sidebar, GridUnitType.Pixel);

        // Left grid ratios as star weights
        var colRatio = _state.LeftColumnRatio;
        if (colRatio <= 0 || colRatio >= 1) colRatio = 0.5;
        colRatio = Clamp(colRatio, 0.15, 0.85);

        var rowRatio = _state.LeftRowRatio;
        if (rowRatio <= 0 || rowRatio >= 1) rowRatio = 0.5;
        rowRatio = Clamp(rowRatio, 0.15, 0.85);

        LeftColLeft.Width = new GridLength(Math.Max(1, colRatio * 100.0), GridUnitType.Star);
        LeftColRight.Width = new GridLength(Math.Max(1, (1.0 - colRatio) * 100.0), GridUnitType.Star);

        LeftRowTop.Height = new GridLength(Math.Max(1, rowRatio * 100.0), GridUnitType.Star);
        LeftRowBottom.Height = new GridLength(Math.Max(1, (1.0 - rowRatio) * 100.0), GridUnitType.Star);
    }

    private void CaptureLayoutToState()
    {
        try
        {
            _state.SidebarWidth = SidebarCol.ActualWidth;

            var colTotal = LeftColLeft.ActualWidth + LeftColRight.ActualWidth;
            if (colTotal > 50)
            {
                _state.LeftColumnRatio = Clamp(LeftColLeft.ActualWidth / colTotal, 0.05, 0.95);
            }

            var rowTotal = LeftRowTop.ActualHeight + LeftRowBottom.ActualHeight;
            if (rowTotal > 50)
            {
                _state.LeftRowRatio = Clamp(LeftRowTop.ActualHeight / rowTotal, 0.05, 0.95);
            }
        }
        catch { }
    }

    private void ScheduleSave()
    {
        _saveCts?.Cancel();
        _saveCts = new CancellationTokenSource();
        var token = _saveCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(800, token);
                if (token.IsCancellationRequested) return;
                Dispatcher.Invoke(SaveNow);
            }
            catch { }
        }, token);
    }

    private void SaveNow()
    {
        CaptureLayoutToState();

        _state.WindowWidth = (int)Math.Max(0, ActualWidth);
        _state.WindowHeight = (int)Math.Max(0, ActualHeight);

        _config.SaveState(_state);
    }

    private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        ScheduleSave();
    }
}
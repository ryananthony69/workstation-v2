using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WorkstationV2.Models;
using WorkstationV2.Services;

namespace WorkstationV2;

public partial class MainWindow : Window
{
    private readonly ConfigService _config = new();
    private AppState _state = new();
    private ToolsConfig _tools = new();

    private CancellationTokenSource? _saveCts;
    private CancellationTokenSource? _toastCts;

    private string _toolsSearch = string.Empty;

    // Selection state for filtered tools
    private int _selectedToolIndex = -1;
    private List<ToolItem> _filteredTools = new();
    private readonly List<Button> _toolButtons = new();

    private static string ToolsDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkstationV2");

    private static string ToolsFilePath => Path.Combine(ToolsDir, "Tools.json");

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

        _toolsSearch = (ToolsSearchBox.Text ?? string.Empty).Trim();
        BuildToolsGrid();
        ValidateToolsAndShowBanner(_tools);

        ApplyToolsHintFromState();

        Focusable = true;
        Keyboard.Focus(this);
    }

    private void ApplyToolsHintFromState()
    {
        var show = _state.ToolsHintVisible ?? true;
        _state.ToolsHintVisible = show;

        ToolsHintToggle.IsChecked = show;
        ToolsHintText.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ToolsHintToggle_Changed(object sender, RoutedEventArgs e)
    {
        var show = ToolsHintToggle.IsChecked == true;
        ToolsHintText.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        _state.ToolsHintVisible = show;
        ScheduleSave();
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveNow();
    }

    private void ReloadTools_Click(object sender, RoutedEventArgs e) => ReloadToolsFromDisk();

    private void OpenTools_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = _config.LoadToolsSeedIfMissing();
            Directory.CreateDirectory(ToolsDir);

            if (!File.Exists(ToolsFilePath))
            {
                File.WriteAllText(ToolsFilePath, "{\n  \"tools\": []\n}\n", new UTF8Encoding(false));
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{ToolsFilePath}\"",
                UseShellExecute = false
            });

            ShowToast("Opened Tools.json");
        }
        catch (Exception ex)
        {
            ShowToast("Open failed: " + ex.Message);
        }
    }

    private void ExportTools_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var json = SerializeTools(_tools);
            Clipboard.SetText(json);
            ShowToast("Tools copied to clipboard");
        }
        catch (Exception ex)
        {
            ShowToast("Export failed: " + ex.Message);
        }
    }

    private void ImportTools_Click(object sender, RoutedEventArgs e) => ShowImportToolsDialog();

    private void ToolsSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        _toolsSearch = (ToolsSearchBox.Text ?? string.Empty).Trim();

        // Reset selection to first item on new search text.
        _selectedToolIndex = -1;

        BuildToolsGrid();
    }

    private void ToolsSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab && Keyboard.Modifiers == ModifierKeys.None)
        {
            FocusSelectedToolButton();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Down)
        {
            MoveToolSelection(+1, focusButton: false);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Up)
        {
            MoveToolSelection(-1, focusButton: false);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            RunSelectedOrSingleMatch();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            ClearToolsSearchAndReset();
            e.Handled = true;
            return;
        }
    }

    private void ClearToolsSearchAndReset()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(ToolsSearchBox.Text))
            {
                ToolsSearchBox.Text = string.Empty;
            }
            else
            {
                _toolsSearch = string.Empty;
                _selectedToolIndex = -1;
                BuildToolsGrid();
            }

            ToolsSearchBox.Focus();
            ToolsSearchBox.SelectAll();
            ShowToast("Search cleared");
        }
        catch { }
    }

    private void FocusToolsSearch()
    {
        try
        {
            ToolsSearchBox.Focus();
            ToolsSearchBox.SelectAll();
        }
        catch { }
    }

    private static bool IsDescendantOf(DependencyObject? child, DependencyObject ancestor)
    {
        var cur = child;
        while (cur != null)
        {
            if (ReferenceEquals(cur, ancestor)) return true;
            cur = VisualTreeHelper.GetParent(cur);
        }
        return false;
    }

    private bool IsFocusInToolsArea()
    {
        try
        {
            var fe = Keyboard.FocusedElement as DependencyObject;
            if (fe == null) return false;
            if (ReferenceEquals(fe, ToolsSearchBox)) return true;
            return IsDescendantOf(fe, ToolsGrid);
        }
        catch
        {
            return false;
        }
    }

    private void MoveToolSelection(int delta, bool focusButton)
    {
        if (_filteredTools.Count == 0)
        {
            _selectedToolIndex = -1;
            ApplyToolSelectionVisuals();
            return;
        }

        if (_selectedToolIndex < 0 || _selectedToolIndex >= _filteredTools.Count)
        {
            _selectedToolIndex = 0;
            ApplyToolSelectionVisuals();
            if (focusButton) FocusSelectedToolButton();
            return;
        }

        var n = _filteredTools.Count;
        var next = _selectedToolIndex + delta;

        if (next < 0) next = n - 1;
        if (next >= n) next = 0;

        _selectedToolIndex = next;
        ApplyToolSelectionVisuals();
        if (focusButton) FocusSelectedToolButton();
    }

    private void FocusSelectedToolButton()
    {
        try
        {
            if (_toolButtons.Count == 0) return;

            var idx = _selectedToolIndex;
            if (idx < 0 || idx >= _toolButtons.Count) idx = 0;

            _selectedToolIndex = idx;
            ApplyToolSelectionVisuals();

            _toolButtons[idx].Focus();
        }
        catch { }
    }

    private void RunSelectedOrSingleMatch()
    {
        if (_filteredTools.Count == 1)
        {
            ExecuteTool(_filteredTools[0]);
            return;
        }

        if (_selectedToolIndex >= 0 && _selectedToolIndex < _filteredTools.Count)
        {
            ExecuteTool(_filteredTools[_selectedToolIndex]);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_toolsSearch))
        {
            if (_filteredTools.Count == 0) ShowToast("No matching tools");
            else ShowToast("Use Up/Down to select");
        }
    }

    private void ShowImportToolsDialog()
    {
        try
        {
            var dlg = new Window
            {
                Title = "Import Tools.json",
                Width = 760,
                Height = 560,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var root = new Grid { Margin = new Thickness(12) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new TextBlock
            {
                Text = "Paste JSON matching the Tools.json schema (root object with \"tools\": [...]).",
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(header, 0);
            root.Children.Add(header);

            var box = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                TextWrapping = TextWrapping.NoWrap,
                FontFamily = new FontFamily("Consolas"),
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid.SetRow(box, 1);
            root.Children.Add(box);

            var error = new TextBlock
            {
                Foreground = Brushes.OrangeRed,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(error, 2);
            root.Children.Add(error);

            var buttons = new DockPanel { LastChildFill = false };

            var cancel = new Button { Content = "Cancel", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 0, 8, 0) };
            cancel.Click += (_, _) => dlg.Close();
            DockPanel.SetDock(cancel, Dock.Right);
            buttons.Children.Add(cancel);

            var save = new Button { Content = "Validate + Save", Padding = new Thickness(12, 6, 12, 6) };
            DockPanel.SetDock(save, Dock.Right);
            buttons.Children.Add(save);

            Grid.SetRow(buttons, 3);
            root.Children.Add(buttons);

            dlg.Content = root;

            try
            {
                var clip = Clipboard.GetText();
                if (!string.IsNullOrWhiteSpace(clip) && (clip.TrimStart().StartsWith("{") || clip.TrimStart().StartsWith("[")))
                {
                    box.Text = clip;
                }
                else
                {
                    box.Text = SerializeTools(_tools);
                }
            }
            catch
            {
                box.Text = SerializeTools(_tools);
            }

            save.Click += (_, _) =>
            {
                try
                {
                    var input = box.Text ?? string.Empty;
                    var (ok, cfg, msg) = TryParseAndValidateTools(input);

                    if (!ok || cfg == null)
                    {
                        error.Text = msg;
                        error.Visibility = Visibility.Visible;
                        return;
                    }

                    Directory.CreateDirectory(ToolsDir);
                    File.WriteAllText(ToolsFilePath, SerializeTools(cfg), new UTF8Encoding(false));

                    _tools = cfg;
                    _selectedToolIndex = -1;

                    BuildToolsGrid();
                    ValidateToolsAndShowBanner(_tools);

                    ShowToast("Tools imported");
                    dlg.Close();
                }
                catch (Exception ex)
                {
                    error.Text = "Import failed: " + ex.Message;
                    error.Visibility = Visibility.Visible;
                }
            };

            dlg.ShowDialog();
        }
        catch (Exception ex)
        {
            ShowToast("Import UI failed: " + ex.Message);
        }
    }

    private static string SerializeTools(ToolsConfig cfg)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(cfg, opts);
    }

    private (bool ok, ToolsConfig? cfg, string message) TryParseAndValidateTools(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return (false, null, "Input is empty.");
        }

        try
        {
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var cfg = JsonSerializer.Deserialize<ToolsConfig>(json, opts);
            if (cfg == null)
            {
                return (false, null, "JSON parsed as null.");
            }

            cfg.Tools ??= new List<ToolItem>();

            var issues = GetToolIssues(cfg);
            if (issues.Count > 0)
            {
                var shown = issues.Take(8).ToList();
                var more = issues.Count - shown.Count;
                var msg = "Validation issues:\n" + string.Join("\n", shown);
                if (more > 0) msg += $"\n(+{more} more)";
                return (false, null, msg);
            }

            return (true, cfg, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, null, "JSON parse failed: " + ex.Message);
        }
    }

    private void ReloadToolsFromDisk()
    {
        try
        {
            _ = _config.LoadToolsSeedIfMissing();

            if (!File.Exists(ToolsFilePath))
            {
                HideToolsError();
                _tools = new ToolsConfig();
                _selectedToolIndex = -1;
                BuildToolsGrid();
                ShowToast("Tools.json not found");
                return;
            }

            var json = File.ReadAllText(ToolsFilePath, new UTF8Encoding(false));
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var cfg = JsonSerializer.Deserialize<ToolsConfig>(json, opts);

            if (cfg == null)
            {
                ShowToolsError("Tools.json parsed as null.");
                ShowToast("Tools reload failed");
                return;
            }

            cfg.Tools ??= new List<ToolItem>();
            _tools = cfg;
            _selectedToolIndex = -1;

            BuildToolsGrid();
            var hasIssues = ValidateToolsAndShowBanner(_tools);
            ShowToast(hasIssues ? "Tools reloaded (with issues)" : "Tools reloaded");
        }
        catch (Exception ex)
        {
            ShowToolsError("Tools.json load failed: " + ex.Message);
            ShowToast("Tools reload failed");
        }
    }

    private List<ToolItem> GetFilteredTools()
    {
        var all = _tools.Tools ?? new List<ToolItem>();
        var q = (_toolsSearch ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(q)) return all;

        return all
            .Where(t => ((t?.Label ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
            .ToList();
    }

    private void BuildToolsGrid()
    {
        ToolsGrid.Children.Clear();
        _toolButtons.Clear();

        _filteredTools = GetFilteredTools();

        if (_filteredTools.Count == 0)
        {
            _selectedToolIndex = -1;
        }
        else
        {
            if (_selectedToolIndex < 0 || _selectedToolIndex >= _filteredTools.Count)
            {
                _selectedToolIndex = 0;
            }
        }

        foreach (var tool in _filteredTools.Take(120))
        {
            var isValid = IsToolValid(tool, out var reason);

            var label = (tool?.Label ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(label)) label = "(invalid tool)";

            var btn = new Button
            {
                Content = label,
                Margin = new Thickness(4),
                Padding = new Thickness(8),
                Tag = tool,
                IsEnabled = isValid,
                ToolTip = isValid ? null : reason
            };

            btn.Click += ToolButton_Click;
            ToolsGrid.Children.Add(btn);
            _toolButtons.Add(btn);
        }

        ApplyToolSelectionVisuals();
    }

    private void ApplyToolSelectionVisuals()
    {
        for (var i = 0; i < _toolButtons.Count; i++)
        {
            var b = _toolButtons[i];
            if (i == _selectedToolIndex)
            {
                b.BorderBrush = SystemColors.HighlightBrush;
                b.BorderThickness = new Thickness(2);
                b.FontWeight = FontWeights.SemiBold;
            }
            else
            {
                b.ClearValue(Button.BorderBrushProperty);
                b.BorderThickness = new Thickness(1);
                b.FontWeight = FontWeights.Normal;
            }
        }
    }

    private void ToolButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b) return;
        if (b.Tag is not ToolItem tool) return;

        try
        {
            var idx = _filteredTools.IndexOf(tool);
            if (idx >= 0)
            {
                _selectedToolIndex = idx;
                ApplyToolSelectionVisuals();
            }
        }
        catch { }

        ExecuteTool(tool);
    }

    private void ExecuteTool(ToolItem tool)
    {
        if (!IsToolValid(tool, out var reason))
        {
            ShowToast("Invalid tool: " + reason);
            return;
        }

        var type = (tool.Type ?? string.Empty).Trim();

        try
        {
            ShowToast($"Tool: {tool.Label ?? type}");

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
                    ShowToast($"Unknown tool type: {type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            ScriptRunner.SetOutput($"Tool error: {ex.Message}");
            ShowToast("Tool failed (see output)");
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
        var sidebar = _state.SidebarWidth <= 0 ? 380 : _state.SidebarWidth;
        sidebar = Clamp(sidebar, 280, 1200);
        SidebarCol.Width = new GridLength(sidebar, GridUnitType.Pixel);

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

    private void Splitter_DragCompleted(object sender, DragCompletedEventArgs e) => ScheduleSave();

    private void ShowToast(string message, int ms = 1500)
    {
        try
        {
            _toastCts?.Cancel();
            _toastCts = new CancellationTokenSource();
            var token = _toastCts.Token;

            ToastText.Text = message ?? string.Empty;
            ToastBorder.Visibility = Visibility.Visible;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(ms, token);
                    if (token.IsCancellationRequested) return;

                    Dispatcher.Invoke(() =>
                    {
                        ToastBorder.Visibility = Visibility.Collapsed;
                        ToastText.Text = string.Empty;
                    });
                }
                catch { }
            }, token);
        }
        catch { }
    }

    private void ShowToolsError(string message)
    {
        ToolsErrorText.Text = message ?? string.Empty;
        ToolsErrorBanner.Visibility = Visibility.Visible;
    }

    private void HideToolsError()
    {
        ToolsErrorText.Text = string.Empty;
        ToolsErrorBanner.Visibility = Visibility.Collapsed;
    }

    private static bool IsToolValid(ToolItem? tool, out string reason)
    {
        reason = "Unknown validation error.";

        if (tool == null)
        {
            reason = "Tool is null.";
            return false;
        }

        var label = (tool.Label ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(label))
        {
            reason = "Missing required field: label.";
            return false;
        }

        var type = (tool.Type ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(type))
        {
            reason = "Missing required field: type.";
            return false;
        }

        switch (type)
        {
            case "OpenUrl":
            {
                var url = tool.Payload?.GetValueOrDefault("url") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(url))
                {
                    reason = "OpenUrl requires payload.url.";
                    return false;
                }
                break;
            }
            case "LaunchApp":
            {
                var exe = tool.Payload?.GetValueOrDefault("exe") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(exe))
                {
                    reason = "LaunchApp requires payload.exe.";
                    return false;
                }
                break;
            }
            case "RunScript":
            {
                var script = tool.Payload?.GetValueOrDefault("script") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(script))
                {
                    reason = "RunScript requires payload.script.";
                    return false;
                }
                break;
            }
            case "FocusTile":
            {
                var t = tool.TileTarget ?? 0;
                if (t < 1 || t > 3)
                {
                    reason = "FocusTile requires tileTarget 1..3.";
                    return false;
                }
                break;
            }
            case "OpenCanvas":
            case "OpenVault":
            case "FocusObsidian":
                break;
            default:
                reason = $"Unknown tool type: {type}";
                return false;
        }

        reason = string.Empty;
        return true;
    }

    private static List<string> GetToolIssues(ToolsConfig cfg)
    {
        var list = cfg.Tools ?? new List<ToolItem>();
        var issues = new List<string>();

        for (var i = 0; i < list.Count; i++)
        {
            if (!IsToolValid(list[i], out var reason))
            {
                var label = (list[i]?.Label ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(label)) label = "(missing label)";
                issues.Add($"#{i + 1} '{label}': {reason}");
            }
        }

        return issues;
    }

    private bool ValidateToolsAndShowBanner(ToolsConfig cfg)
    {
        var issues = GetToolIssues(cfg);
        if (issues.Count == 0)
        {
            HideToolsError();
            return false;
        }

        var shown = issues.Take(6).ToList();
        var more = issues.Count - shown.Count;

        var msg = "Tools.json validation issues:\n" + string.Join("\n", shown);
        if (more > 0) msg += $"\n(+{more} more)";

        ShowToolsError(msg);
        return true;
    }

    private static bool TryGetDigitKey(Key key, out int digit)
    {
        digit = 0;

        if (key >= Key.D0 && key <= Key.D9)
        {
            digit = (int)(key - Key.D0);
            return true;
        }

        if (key >= Key.NumPad0 && key <= Key.NumPad9)
        {
            digit = (int)(key - Key.NumPad0);
            return true;
        }

        return false;
    }

    private void TriggerToolByIndex(int index)
    {
        var list = _tools.Tools ?? new List<ToolItem>();
        if (index < 0 || index >= list.Count)
        {
            ShowToast($"No tool at #{index + 1}");
            return;
        }

        ExecuteTool(list[index]);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            var mods = Keyboard.Modifiers;

            if (mods == ModifierKeys.Control && e.Key == Key.F)
            {
                FocusToolsSearch();
                ShowToast("Tools search");
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (IsFocusInToolsArea() || !string.IsNullOrWhiteSpace(ToolsSearchBox.Text))
                {
                    ClearToolsSearchAndReset();
                    e.Handled = true;
                    return;
                }
            }

            if (IsFocusInToolsArea() && !ReferenceEquals(Keyboard.FocusedElement, ToolsSearchBox))
            {
                if (e.Key == Key.Down)
                {
                    MoveToolSelection(+1, focusButton: true);
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Up)
                {
                    MoveToolSelection(-1, focusButton: true);
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    RunSelectedOrSingleMatch();
                    e.Handled = true;
                    return;
                }
            }

            if (!TryGetDigitKey(e.Key, out var digit)) return;

            if (mods == ModifierKeys.Control)
            {
                if (digit == 1) { FocusTile(1); ShowToast("Focus Tile 1"); e.Handled = true; }
                else if (digit == 2) { FocusTile(2); ShowToast("Focus Tile 2"); e.Handled = true; }
                else if (digit == 3) { FocusTile(3); ShowToast("Focus Tile 3"); e.Handled = true; }
                return;
            }

            if (mods == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                if (digit >= 1 && digit <= 9)
                {
                    TriggerToolByIndex(digit - 1);
                    e.Handled = true;
                }
                return;
            }
        }
        catch { }
    }
}
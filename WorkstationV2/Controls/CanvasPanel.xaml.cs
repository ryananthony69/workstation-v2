using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WorkstationV2.Services;

namespace WorkstationV2.Controls;

public partial class CanvasPanel : UserControl
{
    private Action<string, string>? _onChanged;
    private Action? _onDirty;

    public CanvasPanel()
    {
        InitializeComponent();
        VaultBox.TextChanged += (_, _) => NotifyChanged();
        CanvasBox.TextChanged += (_, _) => NotifyChanged();
    }

    public void Initialize(string vaultPath, string canvasPath, Action<string, string> onChanged, Action onDirty)
    {
        _onChanged = onChanged;
        _onDirty = onDirty;
        VaultBox.Text = vaultPath ?? string.Empty;
        CanvasBox.Text = canvasPath ?? string.Empty;
    }

    private void NotifyChanged()
    {
        _onChanged?.Invoke(VaultBox.Text ?? string.Empty, CanvasBox.Text ?? string.Empty);
    }

    private void BrowseVault_Click(object sender, RoutedEventArgs e)
    {
        // WPF-only folder pick workaround:
        // Navigate to a folder and click Open; we use DirectoryName of a dummy file.
        var initial = (VaultBox.Text ?? string.Empty).Trim();
        var dlg = new OpenFileDialog
        {
            Title = "Select Obsidian Vault Folder",
            CheckFileExists = false,
            CheckPathExists = true,
            ValidateNames = false,
            FileName = "Select Folder",
            Filter = "Folder|*.",
            InitialDirectory = Directory.Exists(initial) ? initial : null
        };

        if (dlg.ShowDialog() == true)
        {
            var folder = Path.GetDirectoryName(dlg.FileName);
            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                VaultBox.Text = folder;
            }
        }
    }

    private void PickCanvas_Click(object sender, RoutedEventArgs e)
    {
        var initial = (VaultBox.Text ?? string.Empty).Trim();
        var dlg = new OpenFileDialog
        {
            Title = "Select Canvas (.canvas)",
            Filter = "Obsidian Canvas (*.canvas)|*.canvas|All files (*.*)|*.*",
            CheckFileExists = true,
            InitialDirectory = Directory.Exists(initial) ? initial : null
        };

        if (dlg.ShowDialog() == true)
        {
            CanvasBox.Text = dlg.FileName;
        }
    }

    public void OpenCanvas(string canvasPath)
    {
        var path = string.IsNullOrWhiteSpace(canvasPath) ? (CanvasBox.Text ?? string.Empty) : canvasPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

        var uri = "obsidian://open?path=" + Uri.EscapeDataString(path);
        Process.Start(new ProcessStartInfo { FileName = uri, UseShellExecute = true });
        _onDirty?.Invoke();
    }

    public void OpenVault(string vaultPath)
    {
        var path = string.IsNullOrWhiteSpace(vaultPath) ? (VaultBox.Text ?? string.Empty) : vaultPath;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;

        Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{path}\"", UseShellExecute = true });
        _onDirty?.Invoke();
    }

    private void OpenCanvas_Click(object sender, RoutedEventArgs e) => OpenCanvas(string.Empty);
    private void OpenVault_Click(object sender, RoutedEventArgs e) => OpenVault(string.Empty);

    private void FocusObsidian_Click(object sender, RoutedEventArgs e)
    {
        WindowActivator.FocusProcessMainWindow("Obsidian");
    }
}
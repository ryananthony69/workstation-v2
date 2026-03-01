using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkstationV2.Controls;

public partial class ScriptRunner : UserControl
{
    public ScriptRunner()
    {
        InitializeComponent();
    }

    public void SetInput(string text)
    {
        InputBox.Text = text ?? string.Empty;
        InputBox.CaretIndex = InputBox.Text.Length;
        InputBox.Focus();
    }

    public void SetOutput(string text)
    {
        OutputBox.Text = text ?? string.Empty;
        TryCopy(OutputBox.Text);
    }

    public void RunCurrent() => RunScript(InputBox.Text ?? string.Empty);

    private void Run_Click(object sender, RoutedEventArgs e) => RunCurrent();

    private void InputBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            RunCurrent();
            e.Handled = true;
        }
    }

    private async void RunScript(string script)
    {
        script = script ?? string.Empty;
        if (string.IsNullOrWhiteSpace(script)) return;

        RunButton.IsEnabled = false;
        StatusText.Text = "Running...";

        try
        {
            var output = await Task.Run(() => RunPowerShell(script));
            OutputBox.Text = output;
            TryCopy(output);
            StatusText.Text = "Done (output copied).";
        }
        catch (Exception ex)
        {
            OutputBox.Text = ex.ToString();
            TryCopy(OutputBox.Text);
            StatusText.Text = "Failed (output copied).";
        }
        finally
        {
            RunButton.IsEnabled = true;
        }
    }

    private static string RunPowerShell(string script)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -ExecutionPolicy Bypass -Command -",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var p = new Process { StartInfo = psi };
        p.Start();

        using (var sw = p.StandardInput)
        {
            sw.Write(script);
            sw.WriteLine();
        }

        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();

        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(stdout)) sb.AppendLine(stdout.TrimEnd());
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine("ERRORS:");
            sb.AppendLine(stderr.TrimEnd());
        }

        return sb.ToString().TrimEnd();
    }

    private static void TryCopy(string text)
    {
        try
        {
            if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
        }
        catch { }
    }
}
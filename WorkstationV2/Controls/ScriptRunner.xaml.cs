using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Management.Automation;

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
            var output = await Task.Run(() =>
            {
                var sb = new StringBuilder();
                using var ps = PowerShell.Create();
                ps.AddScript(script);

                var results = ps.Invoke();

                foreach (var r in results)
                {
                    if (r != null) sb.AppendLine(r.ToString());
                }

                if (ps.Streams.Error.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("ERRORS:");
                    foreach (var err in ps.Streams.Error)
                    {
                        sb.AppendLine(err.ToString());
                    }
                }

                return sb.ToString().TrimEnd();
            });

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

    private void TryCopy(string text)
    {
        try
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }
        catch { }
    }
}
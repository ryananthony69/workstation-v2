using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace WorkstationV2;

public partial class App : Application
{
    private static string CrashDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WorkstationV2");

    private static string CrashLogPath => Path.Combine(CrashDir, "crash.log");

    protected override void OnStartup(StartupEventArgs e)
    {
        Directory.CreateDirectory(CrashDir);

        DispatcherUnhandledException += (_, args) =>
        {
            try
            {
                LogException("DispatcherUnhandledException", args.Exception);
                MessageBox.Show(args.Exception.ToString(), "Workstation v2 - Unhandled UI Exception");
            }
            catch { }
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            try
            {
                if (args.ExceptionObject is Exception ex)
                {
                    LogException("AppDomain.UnhandledException", ex);
                }
                else
                {
                    LogText("AppDomain.UnhandledException", args.ExceptionObject?.ToString() ?? "(null)");
                }
            }
            catch { }
        };

        base.OnStartup(e);
    }

    private static void LogException(string kind, Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine("==== " + DateTime.Now.ToString("u") + " ====");
        sb.AppendLine(kind);
        sb.AppendLine(ex.ToString());
        sb.AppendLine();

        File.AppendAllText(CrashLogPath, sb.ToString(), new UTF8Encoding(false));
    }

    private static void LogText(string kind, string text)
    {
        var sb = new StringBuilder();
        sb.AppendLine("==== " + DateTime.Now.ToString("u") + " ====");
        sb.AppendLine(kind);
        sb.AppendLine(text);
        sb.AppendLine();

        File.AppendAllText(CrashLogPath, sb.ToString(), new UTF8Encoding(false));
    }
}
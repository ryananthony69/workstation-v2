using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WorkstationV2.Services;

public static class WindowActivator
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    public static void FocusProcessMainWindow(string processName)
    {
        try
        {
            var procs = Process.GetProcessesByName(processName);
            foreach (var p in procs)
            {
                if (p.MainWindowHandle == IntPtr.Zero) continue;

                ShowWindow(p.MainWindowHandle, SW_RESTORE);
                SetForegroundWindow(p.MainWindowHandle);
                return;
            }
        }
        catch { }
    }
}
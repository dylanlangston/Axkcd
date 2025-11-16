#if LINUX
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AvaloniaXKCD.Desktop;

public partial class DesktopSystemActions
{
    internal bool HandleErrorLinux(Exception error)
    {
        string errorMessage = $"[ERROR] {error.Message}\n{error.StackTrace}";
        byte[] messageBytes = Encoding.UTF8.GetBytes(errorMessage + "\0");

        // Use the standard C library syslog function
        const int LOG_ERR = 3;
        NativeMethods.syslog(LOG_ERR, messageBytes);
        return true;
    }
}

internal static partial class NativeMethods
{
    // Linux Logging (syslog)
    // Note: On modern Linux, syslog might redirect to journald.
    [LibraryImport("libc", EntryPoint = "syslog")]
    internal static partial void syslog(int priority, byte[] message);
}
#endif

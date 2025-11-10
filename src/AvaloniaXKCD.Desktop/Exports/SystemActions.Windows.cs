#if WINDOWS
using System;
using System.Diagnostics;

namespace AvaloniaXKCD.Desktop;

public partial class DesktopSystemActions
{
    internal bool HandleErrorWindows(Exception error)
    {
        string errorMessage = $"[ERROR] {error.Message}\n{error.StackTrace}";

        try
        {
            // The "Application" event log is generally accessible to user applications.
            // Using the "Application" source is a way to write to the log without needing to create a new source,
            // which would require admin privileges.
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(errorMessage, EventLogEntryType.Error);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
#endif
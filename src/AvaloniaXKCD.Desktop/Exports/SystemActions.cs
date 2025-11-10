using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Desktop;

public partial class DesktopSystemActions : ISystemActions
{
#pragma warning disable CS0067 // The event 'DesktopSystemActions.OnUriChange' is never used
    public event EventHandler<string>? OnUriChange;
#pragma warning restore CS0067

    public Uri GetBaseUri()
    {
        throw new NotImplementedException();
    }

    public bool HandleError(Exception error)
    {
#if WINDOWS
        return HandleErrorWindows(error);
#elif MACOS
        return HandleErrorMac(error);
#elif LINUX
        return HandleErrorLinux(error);
#else
        return false; // Platform not supported
#endif
    }

    public void InvokeOnUriChange(string newUri)
    {
        // Noop
    }

    public void SetTitle(string title)
    {
        // Noop
    }
}
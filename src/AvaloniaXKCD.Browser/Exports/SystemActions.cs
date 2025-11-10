using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Browser;

public partial class BrowserSystemActions : ISystemActions
{
    #region Handle URI Change
    [JSExport]
    public static string? AddOnUriChangeCallback([JSMarshalAs<JSType.Function<JSType.String>>()] Action<string> callback)
    {
        if (App.SystemActions is BrowserSystemActions self && self._uriChangeBridge != null)
        {
            return self._uriChangeBridge.Add(callback);
        }
        else throw new NullReferenceException();
    }

    [JSExport]
    public static void RemoveOnUriChangeCallback(string subscription)
    {
        if (App.SystemActions is BrowserSystemActions self && self._uriChangeBridge != null)
        {
            self._uriChangeBridge.Remove(subscription);
        }
        else throw new NullReferenceException();
    }

    [JSExport]
    public static void InvokeOnUriChangeCallback(string uri)
    {
        if (App.SystemActions is BrowserSystemActions self && self._uriChangeBridge != null)
        {
            self._uriChangeBridge.Invoke(uri);
        }
        else throw new NullReferenceException();
    }

    private readonly CSharpGenericEventBridge<string>? _uriChangeBridge =
        CSharpEventBridgeFactory.GetBridge<string>($"{nameof(BrowserSystemActions)}.${nameof(OnUriChange)}");

    public event EventHandler<string>? OnUriChange
    {
        add
        {
            if (value != null) _uriChangeBridge?.Add(value);
        }
        remove
        {
            if (value != null) _uriChangeBridge?.Remove(value);
        }
    }

    public void InvokeOnUriChange(string newUri)
    {
        _uriChangeBridge?.Invoke(newUri);
    }
    #endregion

    [JSImport("globalThis.alert")]
    internal static partial Task Alert([JSMarshalAs<JSType.String>] string message);

    [JSImport("globalThis.location.toString")]
    internal static partial string GetWindowLocationHref();

    [JSImport("setTitle", "interop")]
    internal static partial void SetTitleInternal(string message);

    public Uri GetBaseUri()
    {
        return new Uri(GetWindowLocationHref());
    }

    public bool HandleError(Exception error)
    {
        Alert(error.ToString()).GetAwaiter().GetResult();
        return true;
    }

    public void SetTitle(string title)
    {
        SetTitleInternal(title);
    }
}
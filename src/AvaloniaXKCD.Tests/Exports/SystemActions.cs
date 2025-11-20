namespace AvaloniaXKCD.Tests;

public partial class NullrSystemActions : ISystemActions
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
        return true;
    }

    public void InvokeOnUriChange(string newUri)
    {
        // NoOp
    }

    public void SetTitle(string title)
    {
        // NoOp
    }
}

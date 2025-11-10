namespace AvaloniaXKCD.Tests;

public partial class NullrSystemActions : ISystemActions
{
    public event EventHandler<string>? OnUriChange;

    public Uri GetBaseUri()
    {
        throw new NotImplementedException();
    }

    public bool HandleError(Exception error)
    {
        return true;
    }

    public void SetTitle(string title)
    {
        throw new NotImplementedException();
    }
}
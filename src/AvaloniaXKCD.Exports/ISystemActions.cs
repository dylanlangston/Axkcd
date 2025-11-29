namespace AvaloniaXKCD.Exports;

public interface ISystemActions : IExport
{
    public bool HandleError(Exception error);
    public Uri GetBaseUri();
    public event EventHandler<string>? OnUriChange;
    public void InvokeOnUriChange(string newUri);
    public void SetTitle(string title);
}

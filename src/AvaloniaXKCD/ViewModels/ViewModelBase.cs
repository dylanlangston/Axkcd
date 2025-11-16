namespace AvaloniaXKCD.ViewModels;

public abstract class ViewModelBase : ObservableObject, IViewModelBase, IDisposable
{
    public virtual Task OnLoad() => Task.CompletedTask;

    private bool _disposedValue;

    public void Dispose()
    {
        Dispose(!_disposedValue);
        _disposedValue = true;
        GC.SuppressFinalize(this);
    }

    // Protected implementation of Dispose pattern.
    protected virtual void Dispose(bool disposing) { }
}

public abstract class ViewModelBaseWithDialog : ViewModelBase, IViewModelBaseWithDialog
{
    public virtual IViewModelDialogBase? DialogViewModel
    {
        get => null;
    }
}

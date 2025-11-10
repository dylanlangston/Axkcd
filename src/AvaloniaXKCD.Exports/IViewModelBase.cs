namespace AvaloniaXKCD.Exports
{
    public interface IViewModelBase : IDisposable
    {
        public Task OnLoad();
    }

    public interface IViewModelBaseWithDialog : IViewModelBase
    {
        public IViewModelDialogBase? DialogViewModel { get; }
    }
}

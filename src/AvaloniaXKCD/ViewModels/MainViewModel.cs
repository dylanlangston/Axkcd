namespace AvaloniaXKCD.ViewModels;

public partial class MainViewModel : ViewModelBaseWithDialog
{
    public MainViewModel()
    {
        CurrentView = new XKCDViewModel(null);
    }

    public MainViewModel(int? initialComic)
    {
        CurrentView = new XKCDViewModel(initialComic);
    }

    public IViewModelBase CurrentView { get; }

    public void OpenErrorDialog(Exception exception, bool fatal)
    {
        var dialogViewModel = new ErrorDialogViewModel(this, exception, !fatal);
        dialogViewModel.OnSubmit += (s, e) =>
        {
            switch (e.Type)
            {
                case ErrorDialogViewModel.ErrorDialogViewModelEventType.Quit:
                    if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Shutdown(1);
                    }
                    else if (App.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
                    {
                        Environment.Exit(1);
                    }
                    break;
                case ErrorDialogViewModel.ErrorDialogViewModelEventType.Continue:
                    DialogViewModel = null;
                    break;
                default:
                    throw new NotImplementedException();
            }
        };
        DialogViewModel = dialogViewModel;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DialogOpen))]
    [NotifyPropertyChangedFor(nameof(CloseDialogOnClickAway))]
    IViewModelDialogBase? _dialogViewModel;

    public bool DialogOpen
    {
        get => DialogViewModel != null;
        set
        {
            if (!value)
            {
                DialogViewModel = null;
            }
        }
    }

    public bool CloseDialogOnClickAway
    {
        get => DialogViewModel?.CanClose ?? true;
    }
}

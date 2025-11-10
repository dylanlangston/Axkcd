namespace AvaloniaXKCD.ViewModels;

public partial class XKCDViewModel : ViewModelBase
{
    private readonly XKCDClient _xkcdClient = new(new(App.Settings.Get(XKCDSettingsContext.Default.XKCDSettings)!.BaseURL));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ComicLoaded))]
    private XKCDComicModel? _currentComic;

    [ObservableProperty]
    private bool _dialogOpen;

    [ObservableProperty]
    private string? _imageSource;

    public bool ComicLoaded => CurrentComic != null;

    partial void OnCurrentComicChanged(XKCDComicModel? value)
    {
        if (value != null)
        {
            App.SystemActions.SetTitle($"AXKCD: {value.Title}");
            App.SystemActions.InvokeOnUriChange($"{value.Num}");
            ImageSource = $"{App.Settings.Get(XKCDSettingsContext.Default.XKCDSettings)!.BaseURL}{value.Img2x}?comic={value.Num}";
        }
        else
        {
            ImageSource = null;
        }
    }

    private int? _initialComic;
    public XKCDViewModel(int? initialComic)
    {
        _initialComic = initialComic;
        App.SystemActions.OnUriChange += async (object? _, string url) =>
        {
            if (!string.IsNullOrEmpty(url) &&
                int.TryParse(url, out int comicNumber) &&
                comicNumber > 0 &&
                comicNumber != _currentComic?.Num)
            {
                await GoTo(comicNumber);
            }
        };
    }

    public override async Task OnLoad()
    {
        var comic = _initialComic.HasValue ? await _xkcdClient.GetComic(_initialComic.Value) : await _xkcdClient.Latest();
        if (comic != null)
        {
            CurrentComic = new XKCDComicModel(comic);
            App.Logger.LogInformation($"Current Comic: {CurrentComic.Num}");
        }
    }

    private async Task LoadComic(Func<Task<IXKCDComic>> comicLoader)
    {
        CurrentComic = null;
        var comic = await comicLoader();
        if (comic != null)
        {
            CurrentComic = new XKCDComicModel(comic);
        }
    }

    [RelayCommand]
    private Task GetFirst() => LoadComic(() => _xkcdClient.GetComic(1));

    [RelayCommand]
    private Task GetPrevious()
    {
        if (CurrentComic is null || CurrentComic.Num <= 1) return Task.CompletedTask;
        var previousComicNum = CurrentComic.Num - 1;
        return LoadComic(() => _xkcdClient.GetComic(previousComicNum));
    }

    [RelayCommand]
    private Task GetRandom() => LoadComic(_xkcdClient.Random);

    [RelayCommand]
    private Task GoTo(object number)
    {
        DialogHost.GetDialogSession("GotoComicDialog")?.Close(false);
        return LoadComic(() => _xkcdClient.GetComic(Convert.ToInt32(number)));
    }

    [RelayCommand]
    private async Task GetNext()
    {
        if (CurrentComic is null) return;

        var latestComicNum = (await _xkcdClient.Latest())?.Num;
        if (latestComicNum.HasValue && CurrentComic.Num >= latestComicNum.Value) return;
        var nextComicNum = CurrentComic.Num + 1;
        await LoadComic(() => _xkcdClient.GetComic(nextComicNum));
    }

    [RelayCommand]
    private Task GetLast() => LoadComic(_xkcdClient.Latest);

    [RelayCommand]
    private void Open()
    {
        if (CurrentComic != null && !CurrentComic.TryOpen(new Uri("https://xkcd.com/", UriKind.Absolute)))
        {
            App.Logger.LogError("Failed to open comic!");
        }
    }

    [RelayCommand]
    private void Explain()
    {
        if (CurrentComic != null && !CurrentComic.Explain())
        {
            App.Logger.LogError("Failed to open explain!");
        }
    }

    [RelayCommand]
    private async Task CopyURI()
    {
        if (CurrentComic is null) return;
        var url = CurrentComic.GetUri(new Uri("https://xkcd.com/", UriKind.Absolute));
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.Clipboard != null)
            {
                await desktop.MainWindow.Clipboard.SetTextAsync(url.AbsoluteUri);
            }
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView && TopLevel.GetTopLevel(singleView.MainView)?.Clipboard != null)
            {
                await TopLevel.GetTopLevel(singleView.MainView)!.Clipboard!.SetTextAsync(url.AbsoluteUri);
            }
        }
        catch (Exception er)
        {
            App.Current?.OnError((er, false));
        }
    }
}
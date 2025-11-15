namespace AvaloniaXKCD;

public partial class App : Application
{
    public static readonly IConfig Config = ExportContainer.Get<IConfig>().ThrowIfNull();

    public static readonly ISettingsRepo Settings = ExportContainer.Get<ISettingsRepo>().ThrowIfNull();
    public static readonly ILogger Logger = ExportContainer.Get<ILogger>().ThrowIfNull();
    public static readonly ISystemActions SystemActions = ExportContainer.Get<ISystemActions>().ThrowIfNull();

    internal static readonly ServiceProvider ServiceProvider = new();

    public new static App? Current
    {
        get => (App?)Application.Current;
    }

    private readonly CancellationToken? _cancellation;
    private readonly ParsedArguments? _args;

    public App() : base()
    {
        _args = null;
        _cancellation = null;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    public App(ParsedArguments parsedArguments, CancellationToken cancellationToken = default) : base()
    {
        _args = parsedArguments;
        _cancellation = cancellationToken;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;
        OnError((exception!, e.IsTerminating));
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        OnError((e.Exception, true));
        e.SetObserved();
    }

    public void OnError(AppErrorEventArgs e) => Current?.ErrorOccured?.Invoke(Current, e);

    event EventHandler<AppErrorEventArgs>? ErrorOccured;

    public override void Initialize()
    {
        Logger.LogInformation("App Framework Initialization Started");


        Logger.IfShouldLogInformation(() =>
        {
            var xkcdSettings = Settings.Get(XKCDSettingsContext.Default.XKCDSettings);

            return $"XKCD BaseURL: '{xkcdSettings?.BaseURL}'";
        });

        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddHttpClient();
        ServiceProvider.ConfigureServices(serviceCollection);

        SetupViewModel(GetVM());

        base.OnFrameworkInitializationCompleted();

        Logger.LogInformation("App Framework Initialization Completed");
    }


    public IViewModelBase GetVM()
    {
        var VM = new MainViewModel(_args?.ComicNumber);
        ErrorOccured += (s, e) =>
        {
            if (e.Fatal) SystemActions.HandleError(e.Exception);
            VM.OpenErrorDialog(e.Exception, e.Fatal);
        };
        return VM;
    }

    void SetupViewModel(IViewModelBase viewModel)
    {
        // DisableAvaloniaDataAnnotationValidation();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = viewModel,
            };

            _cancellation?.Register(() =>
            {
                if (!desktop.TryShutdown(0)) Environment.Exit(1);
            });

            desktop.Exit += (s, e) => Shutdown(viewModel);
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = viewModel
            };
            singleViewPlatform.MainView.Unloaded += (s, e) => Shutdown(viewModel);
        }
    }

    // private void DisableAvaloniaDataAnnotationValidation()
    // {
    //     var dataValidationPluginsToRemove =
    //         BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

    //     foreach (var plugin in dataValidationPluginsToRemove)
    //     {
    //         BindingPlugins.DataValidators.Remove(plugin);
    //     }
    // }

    void Shutdown(IViewModelBase viewModel)
    {
        viewModel.Dispose();
        Settings.Dispose();
    }
}
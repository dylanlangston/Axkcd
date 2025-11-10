namespace AvaloniaXKCD;

[ViewLocator]
internal partial class ViewLocator : IDataTemplate
{
    public static ViewLocator Instance = new();

    private partial Dictionary<Type, Type> CreateViewModelViewMap();

    private readonly Lazy<Dictionary<Type, Type>> _viewModelViewMap;

    public ViewLocator()
    {
        _viewModelViewMap = new Lazy<Dictionary<Type, Type>>(CreateViewModelViewMap);
    }

    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var viewModelType = data.GetType();

        try
        {
            if (!_viewModelViewMap.Value.TryGetValue(viewModelType, out var viewType))
                throw new InvalidOperationException($"No view was registered for ViewModel '{viewModelType.FullName}'.");

            var control = (Control?)App.ServiceProvider.Services?.GetService(viewType);
            if (control == null) throw new NullReferenceException($"Failed to find Service '{viewType.FullName}'");

            control.DataContext = data;

            if (data is IViewModelBase viewModel)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try
                    {
                        await viewModel.OnLoad();
                    }
                    catch (Exception e)
                    {
                        OnError(e, false);
                    }
                }).ConfigureAwait(false);
            }

            return control;
        }
        catch (Exception e)
        {
            return OnError(e, true);
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    private Control? OnError(Exception e, bool fatal)
    {
        App.Logger.LogCritical(e);
        App.Current?.OnError((e, fatal));
        return fatal ? new TextBlock()
        {
            Text = "A Fatal Error Occured!",
            Background = Brushes.White,
            Foreground = Brushes.Red
        } : null;
    }
}
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaXKCD.Views;

public partial class NavigationControlsView : UserControl
{
    public static readonly StyledProperty<bool?> EnabledProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        bool?
    >(nameof(Enabled));

    public static readonly StyledProperty<ICommand?> GetFirstCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(GetFirstCommand));

    public static readonly StyledProperty<ICommand?> GetPreviousCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(GetPreviousCommand));

    public static readonly StyledProperty<ICommand?> GetRandomCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(GetRandomCommand));

    public static readonly StyledProperty<ICommand?> ExplainCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(ExplainCommand));

    public static readonly StyledProperty<ICommand?> OpenDialogCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(OpenDialogCommand));

    public static readonly StyledProperty<ICommand?> GetNextCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(GetNextCommand));

    public static readonly StyledProperty<ICommand?> GetLastCommandProperty = AvaloniaProperty.Register<
        NavigationControlsView,
        ICommand?
    >(nameof(GetLastCommand));

    public bool? Enabled
    {
        get => GetValue(EnabledProperty);
        set => SetValue(EnabledProperty, value);
    }

    public ICommand? GetFirstCommand
    {
        get => GetValue(GetFirstCommandProperty);
        set => SetValue(GetFirstCommandProperty, value);
    }

    public ICommand? GetPreviousCommand
    {
        get => GetValue(GetPreviousCommandProperty);
        set => SetValue(GetPreviousCommandProperty, value);
    }

    public ICommand? GetRandomCommand
    {
        get => GetValue(GetRandomCommandProperty);
        set => SetValue(GetRandomCommandProperty, value);
    }

    public ICommand? ExplainCommand
    {
        get => GetValue(ExplainCommandProperty);
        set => SetValue(ExplainCommandProperty, value);
    }

    public ICommand? OpenDialogCommand
    {
        get => GetValue(OpenDialogCommandProperty);
        set => SetValue(OpenDialogCommandProperty, value);
    }

    public ICommand? GetNextCommand
    {
        get => GetValue(GetNextCommandProperty);
        set => SetValue(GetNextCommandProperty, value);
    }

    public ICommand? GetLastCommand
    {
        get => GetValue(GetLastCommandProperty);
        set => SetValue(GetLastCommandProperty, value);
    }

    public NavigationControlsView()
    {
        InitializeComponent();
    }
}

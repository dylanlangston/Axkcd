namespace AvaloniaXKCD.Views;

[Service]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        App.Logger.IfShouldLogInformation(() => "Main Window Initialization Completed");
    }

    // private void Minimize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    // {
    //     this.WindowState = WindowState.Minimized;
    // }
    // private void Maximize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    // {
    //     this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    // }
    // private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    // {
    //     this.Close();
    // }
}

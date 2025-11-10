namespace AvaloniaXKCD.Views;

[Service]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        App.Logger.IfShouldLogInformation(() => "Main View Initialization Completed");
    }
}
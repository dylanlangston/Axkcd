namespace Test.ViewModels
{
    public class HomeViewModel { }

    public class SettingsViewModel { }
}

namespace Test.Views
{
    public class HomeView { }

    public class SettingsView { }
}

namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ViewLocatorAttribute]
    internal partial class ViewLocator
    {
        private partial Dictionary<Type, Type> CreateViewModelViewMap();
    }
}

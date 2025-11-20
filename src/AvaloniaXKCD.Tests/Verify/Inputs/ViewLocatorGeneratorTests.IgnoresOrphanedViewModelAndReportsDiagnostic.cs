namespace TestNamespace
{
    // This ViewModel has no corresponding View.
    public class OrphanViewModel { }

    public class LoginViewModel { }

    public class LoginView { }
}

namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ViewLocatorAttribute]
    internal partial class ViewLocator
    {
        private partial Dictionary<Type, Type> CreateViewModelViewMap();
    }
}

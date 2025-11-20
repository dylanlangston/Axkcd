namespace TestNamespace
{
    // This View has no corresponding ViewModel.
    public class OrphanView { }

    public class ProfileViewModel { }

    public class ProfileView { }
}

namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ViewLocatorAttribute]
    internal partial class ViewLocator
    {
        private partial Dictionary<Type, Type> CreateViewModelViewMap();
    }
}

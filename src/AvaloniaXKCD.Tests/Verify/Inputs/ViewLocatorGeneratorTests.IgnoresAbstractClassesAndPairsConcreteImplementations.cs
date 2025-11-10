namespace TestNamespace
{
    public abstract class BaseViewModel { }
    public abstract class BaseView { }

    // The generator should ignore the abstract base classes above.
    public class DetailsViewModel : BaseViewModel { }
    public class DetailsView : BaseView { }
}

namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ViewLocatorAttribute]
    internal partial class ViewLocator
    {
        private partial Dictionary<Type, Type> CreateViewModelViewMap();
    }
}
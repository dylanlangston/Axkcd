namespace TestNamespace
{
    // No classes that end with ""View"" or ""ViewModel""
    public class MyService { }

    public interface IRepository { }
}

namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ViewLocatorAttribute]
    internal partial class ViewLocator
    {
        private partial Dictionary<Type, Type> CreateViewModelViewMap();
    }
}

namespace Test.Nested
{
    public class AccountViewModel { }
    public class AccountView { }
}

namespace Test.Nested.Sub
{
    public class UserViewModel { }
    public class UserView { }
}

namespace Test.Nested
{
    [AvaloniaXKCD.Generators.ViewLocatorAttribute]
    internal partial class ViewLocator
    {
        private partial Dictionary<Type, Type> CreateViewModelViewMap();
    }
}
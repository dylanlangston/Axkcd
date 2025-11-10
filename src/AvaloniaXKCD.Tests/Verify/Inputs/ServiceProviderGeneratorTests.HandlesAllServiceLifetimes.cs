namespace Test.Services
{
    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Singleton)]
    public class SingletonService { }

    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Scoped)]
    public class ScopedService { }

    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Transient)]
    public class TransientService { }
}

namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ServiceProvider]
    internal partial class ServiceProvider
    {
        public partial IServiceProvider Services { get; }
        public partial void ConfigureServices(ServiceCollection services);
    }
}
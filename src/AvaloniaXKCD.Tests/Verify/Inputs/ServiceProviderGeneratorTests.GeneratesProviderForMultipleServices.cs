namespace Test.Services
{
    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Singleton)]
    public class LoggerService { }

    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Transient)]
    public class DataService { }
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
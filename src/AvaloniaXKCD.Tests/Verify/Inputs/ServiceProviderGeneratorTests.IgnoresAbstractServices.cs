namespace TestNamespace
{
    public abstract class BaseService { }

    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Singleton)]
    public class ConcreteService : BaseService { }

    [AvaloniaXKCD.Generators.ServiceProvider]
    internal partial class ServiceProvider
    {
        public partial IServiceProvider Services { get; }
        public partial void ConfigureServices(ServiceCollection services);
    }
}
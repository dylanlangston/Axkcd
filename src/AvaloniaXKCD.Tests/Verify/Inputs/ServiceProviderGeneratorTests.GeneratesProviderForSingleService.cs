namespace TestNamespace
{
    [AvaloniaXKCD.Generators.Service(AvaloniaXKCD.Generators.ServiceLifetime.Singleton)]
    public class TestService { }

    [AvaloniaXKCD.Generators.ServiceProvider]
    internal partial class ServiceProvider
    {
        public partial IServiceProvider Services { get; }

        public partial void ConfigureServices(ServiceCollection services);
    }
}

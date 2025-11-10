namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ServiceProvider]
    internal partial class ServiceProvider
    {
        public partial IServiceProvider Services { get; }
        public partial void ConfigureServices(ServiceCollection services);
    }
}
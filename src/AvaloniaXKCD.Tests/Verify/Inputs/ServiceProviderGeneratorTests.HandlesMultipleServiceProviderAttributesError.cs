namespace TestNamespace
{
    [AvaloniaXKCD.Generators.ServiceProvider]
    internal partial class FirstServiceProvider
    {
        public partial IServiceProvider Services { get; }
        public partial void ConfigureServices(ServiceCollection services);
    }

    [AvaloniaXKCD.Generators.ServiceProvider]
    internal partial class SecondServiceProvider
    {
        public partial IServiceProvider Services { get; }
        public partial void ConfigureServices(ServiceCollection services);
    }
}
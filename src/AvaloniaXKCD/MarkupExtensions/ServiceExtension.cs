namespace AvaloniaXKCD.MarkupExtensions;

public class ServiceExtension : MarkupExtension
{
    public Type? ServiceType { get; set; }

    public ServiceExtension() { }

    public ServiceExtension(Type serviceType)
    {
        ServiceType = serviceType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        ServiceType.ThrowIfNull();
        var service = App.ServiceProvider.Services?.GetService(ServiceType);
        service.ThrowIfNull();

        return service;
    }
}
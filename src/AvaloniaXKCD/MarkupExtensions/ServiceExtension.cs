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
        if (ServiceType is null)
        {
            throw new InvalidOperationException("The ServiceType must be specified.");
        }

        var service = App.ServiceProvider.Services?.GetService(ServiceType);

        if (service is null)
        {
            throw new InvalidOperationException($"Service of type {ServiceType.FullName} not found.");
        }

        return service;
    }
}
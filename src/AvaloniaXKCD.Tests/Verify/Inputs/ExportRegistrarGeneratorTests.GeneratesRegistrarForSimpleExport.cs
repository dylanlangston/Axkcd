using AvaloniaXKCD.Exports;

namespace TestNamespace
{
    public interface IService : IExport { }

    public class ServiceImpl : IService
    {
        public ServiceImpl() { }
    }
}

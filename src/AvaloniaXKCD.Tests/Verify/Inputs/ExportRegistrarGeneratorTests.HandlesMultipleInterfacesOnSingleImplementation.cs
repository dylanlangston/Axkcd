using AvaloniaXKCD.Exports;

namespace TestNamespace
{
    public interface IService1 : IExport { }
    public interface IService2 : IExport { }
    
    public class MultiService : IService1, IService2 
    { 
        public MultiService() { }
    }
}
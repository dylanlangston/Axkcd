using AvaloniaXKCD.Exports;

namespace TestNamespace
{
    public interface IService : IExport { }
    
    public class ServiceImpl1 : IService 
    { 
        public ServiceImpl1() { }
    }

    public class ServiceImpl2 : IService 
    { 
        public ServiceImpl2() { }
    }
}
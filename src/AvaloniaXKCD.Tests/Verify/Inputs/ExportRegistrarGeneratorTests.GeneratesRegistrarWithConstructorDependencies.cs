using AvaloniaXKCD.Exports;

namespace TestNamespace
{
    // Note: Dependencies don't need to be IExport themselves, 
    // but they must have a parameterless constructor for this generator logic.
    public class Logger 
    { 
        public Logger() { }
    }

    public interface IService : IExport { }

    public class ServiceImpl : IService 
    { 
        public ServiceImpl(Logger logger) { }
    }
}
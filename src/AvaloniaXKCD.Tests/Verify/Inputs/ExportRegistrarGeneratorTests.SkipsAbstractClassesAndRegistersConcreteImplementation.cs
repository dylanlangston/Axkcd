using AvaloniaXKCD.Exports;

namespace TestNamespace
{
    public interface IService : IExport { }

    public abstract class AbstractService : IService { }

    // The generator should ignore AbstractService and only register this one.
    public class ConcreteService : AbstractService
    {
        public ConcreteService() { }
    }
}

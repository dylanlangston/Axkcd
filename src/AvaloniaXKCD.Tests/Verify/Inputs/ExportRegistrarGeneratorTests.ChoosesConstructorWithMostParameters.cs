using AvaloniaXKCD.Exports;

namespace TestNamespace
{
    public class Repo { }

    public class Logger { }

    public interface IService : IExport { }

    public class MyService : IService
    {
        // This constructor should be ignored by the generator.
        public MyService() { }

        // This constructor should be chosen.
        public MyService(Repo repo, Logger logger) { }
    }
}

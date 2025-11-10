using System.Data;

namespace AvaloniaXKCD.Exports;

public partial class ExportContainer
{
    private static readonly Dictionary<string, Lazy<IExport>> _factories = new();

    public static void Add<T>(string name, Func<T> factory) where T : IExport
    {
        if (_factories.ContainsKey(name)) throw new DuplicateNameException(name);
        _factories[name] = new Lazy<IExport>(() => factory());
    }
    public static void Add<T>(Func<T> factory) where T : IExport => Add<T>(typeof(T).FullName, factory);
    public static void Add<T, C>(string name) where T : IExport where C : T, new() => Add<T>(name, () => new C());
    public static void Add<T, C>() where T : IExport where C : T, new() => Add<T>(() => new C());


    public static T? Get<T>(string name) where T : IExport
    {
        if (!_factories.ContainsKey(name)) return default;
        return (T)_factories[name].Value;
    }
    public static T? Get<T>() where T : IExport => Get<T>(typeof(T).FullName);
}
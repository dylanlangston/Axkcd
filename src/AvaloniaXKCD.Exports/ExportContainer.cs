using System.Data;

namespace AvaloniaXKCD.Exports;

public partial class ExportContainer
{
    private static readonly Dictionary<string, List<Lazy<IExport>>> _multiFactories = new();

    public static void Add<T>(string name, Func<T> factory)
        where T : IExport => AddMulti<T>(name, factory);

    public static void Add<T>(Func<T> factory)
        where T : IExport => Add<T>(typeof(T).FullName, factory);

    public static void Add<T, C>(string name)
        where T : IExport
        where C : T, new() => Add<T>(name, () => new C());

    public static void Add<T, C>()
        where T : IExport
        where C : T, new() => Add<T>(() => new C());

    public static void AddMulti<T>(string name, Func<T> factory)
        where T : IExport
    {
        if (!_multiFactories.ContainsKey(name))
        {
            _multiFactories[name] = new List<Lazy<IExport>>();
        }
        _multiFactories[name].Add(new Lazy<IExport>(() => factory()));
    }

    public static void AddMulti<T>(Func<T> factory)
        where T : IExport => AddMulti<T>(typeof(T).FullName, factory);

    public static void AddMulti<T, C>(string name)
        where T : IExport
        where C : T, new() => AddMulti<T>(name, () => new C());

    public static void AddMulti<T, C>()
        where T : IExport
        where C : T, new() => AddMulti<T>(() => new C());

    public static T? Get<T>(string name)
        where T : IExport => GetAllEnumerable<T>(name).SingleOrDefault();

    public static T? Get<T>()
        where T : IExport => Get<T>(typeof(T).FullName);

    public static T[] GetAll<T>(string name)
        where T : IExport => GetAllEnumerable<T>(name).ToArray();

    static IEnumerable<T> GetAllEnumerable<T>(string name)
        where T : IExport
    {
        if (!_multiFactories.TryGetValue(name, out var lazyList))
        {
            return Array.Empty<T>();
        }
        return lazyList.Select(lazy => (T)lazy.Value);
    }

    public static T[] GetAll<T>()
        where T : IExport => GetAll<T>(typeof(T).FullName);
}

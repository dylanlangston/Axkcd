using System.Data;

namespace AvaloniaXKCD.Exports;

public partial class ExportContainer
{
    private static readonly Dictionary<string, Lazy<IExport>> _factories = new();
    private static readonly Dictionary<string, List<Lazy<IExport>>> _multiFactories = new();

    public static void Add<T>(string name, Func<T> factory) where T : IExport
    {
        if (_factories.ContainsKey(name)) throw new DuplicateNameException(name);
        _factories[name] = new Lazy<IExport>(() => factory());
    }
    
    public static void Add<T>(Func<T> factory) where T : IExport => Add<T>(typeof(T).FullName, factory);
    
    public static void Add<T, C>(string name) where T : IExport where C : T, new() => Add<T>(name, () => new C());
    
    public static void Add<T, C>() where T : IExport where C : T, new() => Add<T>(() => new C());

    /// <summary>
    /// Adds an implementation to the multi-registration list for an interface.
    /// This allows multiple implementations of the same interface to be registered.
    /// </summary>
    public static void AddMulti<T>(string name, Func<T> factory) where T : IExport
    {
        if (!_multiFactories.ContainsKey(name))
        {
            _multiFactories[name] = new List<Lazy<IExport>>();
        }
        _multiFactories[name].Add(new Lazy<IExport>(() => factory()));
    }
    
    public static void AddMulti<T>(Func<T> factory) where T : IExport => AddMulti<T>(typeof(T).FullName, factory);
    
    public static void AddMulti<T, C>(string name) where T : IExport where C : T, new() => AddMulti<T>(name, () => new C());
    
    public static void AddMulti<T, C>() where T : IExport where C : T, new() => AddMulti<T>(() => new C());

    public static T? Get<T>(string name) where T : IExport
    {
        if (!_factories.ContainsKey(name)) return default;
        return (T)_factories[name].Value;
    }
    
    public static T? Get<T>() where T : IExport => Get<T>(typeof(T).FullName);

    /// <summary>
    /// Gets all registered implementations for the specified interface type.
    /// Returns an empty array if no implementations are registered.
    /// </summary>
    public static T[] GetAll<T>(string name) where T : IExport
    {
        if (!_multiFactories.ContainsKey(name))
        {
            return Array.Empty<T>();
        }
        return _multiFactories[name].Select(lazy => (T)lazy.Value).ToArray();
    }
    
    public static T[] GetAll<T>() where T : IExport => GetAll<T>(typeof(T).FullName);
}
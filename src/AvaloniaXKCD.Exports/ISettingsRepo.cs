using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AvaloniaXKCD.Exports;

public interface ISettingsRepo : IExport, IDisposable
{
    void Load();
    void Save();

    T? Get<T>(JsonTypeInfo<T> typeInfo) where T : class, new();
    void Set<T>(T obj, JsonTypeInfo<T> typeInfo);
}

public abstract class BaseSettingsRepo : ISettingsRepo
{
    public abstract void Save();

    public abstract void Load();

    public abstract T? Get<T>(JsonTypeInfo<T> typeInfo) where T : class, new();
    public abstract void Set<T>(T obj, JsonTypeInfo<T> typeInfo);

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Save();
        }
    }
}
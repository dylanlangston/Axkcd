using System;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Browser;

public sealed partial class LocalStorageSettingsRepo : BaseSettingsRepo
{
    private const string SettingsKey = "settings";
    private JsonObject _settings = new();

    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public LocalStorageSettingsRepo()
    {
        Load();
    }

    // JS-imported localStorage
    [JSImport("globalThis.localStorage.getItem")]
    internal static partial Task<string?> LocalStorage_GetItem(string key);

    [JSImport("globalThis.localStorage.setItem")]
    internal static partial Task LocalStorage_SetItem(string key, string value);

    [JSImport("globalThis.localStorage.removeItem")]
    internal static partial Task LocalStorage_RemoveItem(string key);

    [JSImport("globalThis.localStorage.key")]
    internal static partial Task<string?> LocalStorage_Key(int index);

    public override void Save()
    {
        var jsonString = _settings.ToJsonString(_serializerOptions);
        LocalStorage_SetItem(SettingsKey, jsonString);
    }

    public override void Load()
    {
        // var jsonString = LocalStorage_GetItem(SettingsKey);
        // if (string.IsNullOrEmpty(jsonString))
        // {
        //     return;
        // }

        // try
        // {
        //     var jsonNode = JsonNode.Parse(jsonString);
        //     _settings = jsonNode as JsonObject ?? new JsonObject();
        // }
        // catch (Exception)
        // {
        //     _settings = new JsonObject();
        // }
    }

    public override T Get<T>(JsonTypeInfo<T> typeInfo)
    {
        return _settings.Deserialize<T>(typeInfo)!;
    }

    public override void Set<T>(T obj, JsonTypeInfo<T> typeInfo)
    {
        var newSettingsNode = JsonSerializer.SerializeToNode(obj, typeInfo);

        if (!JsonNode.DeepEquals(_settings, newSettingsNode))
        {
            _settings = newSettingsNode as JsonObject ?? new JsonObject();
            Save();
        }
    }
}

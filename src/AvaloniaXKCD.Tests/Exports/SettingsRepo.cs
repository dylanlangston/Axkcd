using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AvaloniaXKCD.Exports;

namespace AvaloniaXKCD.Tests;

public sealed class InMemorySettingsRepo : BaseSettingsRepo
{
    private JsonObject _settings = new();
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public override void Save()
    {
        // No-op: Nothing to save to persistent storage.
    }

    public override void Load()
    {
        // No-op: Settings are only stored in memory for the application's lifetime.
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
        }
    }
}

using System;
using System.IO;
using AvaloniaXKCD.Exports;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AvaloniaXKCD.Desktop;

public sealed class JsonSettingsRepo : BaseSettingsRepo
{
    private const string SettingsFileName = "settings.json";
    private readonly string _settingsFilePath;
    private JsonObject _settings = new();

    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public JsonSettingsRepo()
    {
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        Load();
    }

    private bool _isDirty = false;
    public override void Save()
    {
        if (!_isDirty)
        {
            return;
        }

        var jsonString = _settings.ToJsonString(_serializerOptions);
        File.WriteAllText(_settingsFilePath, jsonString);

        _isDirty = false;
    }

    public override void Load()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return;
        }

        try
        {
            var jsonString = File.ReadAllText(_settingsFilePath);
            var jsonNode = JsonNode.Parse(jsonString);

            _settings = jsonNode as JsonObject ?? new JsonObject();
        }
        catch (Exception)
        {
            _settings = new JsonObject();
        }

        _isDirty = false;
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
            _isDirty = true;
        }
    }
}
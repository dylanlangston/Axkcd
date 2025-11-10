using System.Text.Json.Serialization;

namespace XKCDCore;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
)]
[JsonSerializable(typeof(XKCDComic))]
internal partial class XKCDJsonContext : JsonSerializerContext
{
}

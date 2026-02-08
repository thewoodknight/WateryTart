using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WateryTart.MusicAssistant.Models.Auth;

namespace WateryTart.Core.Settings;

public class MusicAssistantCredentialsConverter : JsonConverter<IMusicAssistantCredentials>
{
    public override IMusicAssistantCredentials? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use JsonTypeInfo if available to avoid RequiresUnreferencedCode warning
        var typeInfo = (JsonTypeInfo<MusicAssistantCredentials>?)options.GetTypeInfo(typeof(MusicAssistantCredentials));
        if (typeInfo != null)
        {
            return JsonSerializer.Deserialize(ref reader, typeInfo);
        }
        // Fallback for when typeInfo is not available
#pragma warning disable IL2026
#pragma warning disable IL3050
        return JsonSerializer.Deserialize<MusicAssistantCredentials>(ref reader, options);
#pragma warning restore IL3050
#pragma warning restore IL2026
    }

    public override void Write(Utf8JsonWriter writer, IMusicAssistantCredentials value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value as MusicAssistantCredentials, options);
    }
}

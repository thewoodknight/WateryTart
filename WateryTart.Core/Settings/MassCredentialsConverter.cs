using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using WateryTart.Service.MassClient.Models.Auth;

namespace WateryTart.Core.Settings;

public class MassCredentialsConverter : JsonConverter<IMassCredentials>
{
    public override IMassCredentials? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize<MassCredentials>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, IMassCredentials value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value as MassCredentials, options);
    }
}
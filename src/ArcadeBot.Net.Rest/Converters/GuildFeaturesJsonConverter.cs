using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ArcadeBot.Core.Entities.Guilds;

namespace ArcadeBot.Converters;

public class GuildFeaturesJsonConverter : JsonConverter<GuildFeatures>
{
    public override GuildFeatures? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException();
        var arr = new List<string>();

        reader.Read();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            arr.Add(JsonSerializer.Deserialize<string>(ref reader, options)!);
            reader.Read();
        }
        GuildFeature features = GuildFeature.None;
        List<string> experimental = new();
        foreach (var item in arr)
        {
            if (Enum.TryParse<GuildFeature>(string.Concat(item.Split('_')), true, out var result))
                features |= result;
            else
                experimental.Add(item);
        }
        return new GuildFeatures(features, experimental.ToArray());
    }

    public override void Write(Utf8JsonWriter writer, GuildFeatures value, JsonSerializerOptions options)
    {
        var enumValues = Enum.GetValues<GuildFeature>();
        writer.WriteStartArray();

        foreach (var enumValue in enumValues)
        {
            if (enumValue is GuildFeature.None)
                continue;
            if (value.Value.HasFlag(enumValue))
                writer.WriteStringValue(ToApiString(enumValue));
        }

        writer.WriteEndArray();
    }

    private static string ToApiString(GuildFeature feature)
    {
        var sb = new StringBuilder();
        bool firstChar = true;
        foreach (var c in feature.ToString().ToCharArray())
        {
            if (char.IsUpper(c))
            {
                if (firstChar) firstChar = false;
                else sb.Append("_");
                sb.Append(c);
            }
            else 
                sb.Append(char.ToUpper(c));
        }
        return sb.ToString();
    }
}

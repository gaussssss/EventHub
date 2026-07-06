using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventHub.Api.Json;

/// <summary>
/// Sérialise toutes les <see cref="DateTime"/> en ISO-8601 **UTC avec suffixe
/// « Z »**. Les dates du domaine sont écrites en <c>DateTime.UtcNow</c> mais
/// EF/SQLite les relit en <c>Kind=Unspecified</c> : sans ce convertisseur elles
/// partiraient sans « Z » et l'app mobile les interpréterait comme locales
/// (d'où des durées « il y a -239 min »). On force donc le Kind UTC à l'écriture.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetDateTime();

    public override void Write(
        Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(
            DateTime.SpecifyKind(value, DateTimeKind.Utc)
                .ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
}

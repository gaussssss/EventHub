using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventHub.Api.Json;

/// <summary>
/// Convention de fuseau de l'API : **tout est UTC**, à l'écriture comme à la
/// lecture ; l'affichage en heure locale est la responsabilité des clients.
///
/// Écriture : ISO-8601 UTC avec suffixe « Z ». Les dates du domaine sont écrites
/// en <c>DateTime.UtcNow</c> mais EF/SQLite les relit en <c>Kind=Unspecified</c> :
/// sans ce convertisseur elles partiraient sans « Z » et l'app mobile les
/// interpréterait comme locales (d'où des durées « il y a -239 min »).
///
/// Lecture : normalisée en UTC quel que soit le format reçu, avec offset (« Z »,
/// « +02:00 » → converti), ou sans offset → **réputé UTC** (contrat d'API), et
/// jamais dépendant du fuseau de la machine serveur (le `GetDateTime()` natif
/// convertissait les offsets vers l'heure locale du serveur).
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTimeOffset.Parse(
                reader.GetString()!,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)
            .UtcDateTime;

    public override void Write(
        Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(
            DateTime.SpecifyKind(value, DateTimeKind.Utc)
                .ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
}

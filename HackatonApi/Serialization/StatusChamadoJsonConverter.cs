using System.Text.Json;
using System.Text.Json.Serialization;
using Hackaton.Domain.Enums;

namespace HackatonApi.Serialization;

public sealed class StatusChamadoJsonConverter : JsonConverter<StatusChamado>
{
    public override StatusChamado Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && StatusChamadoTexto.TryParse(reader.GetString(), out var status))
            return status;
        throw new JsonException("Status inválido. Use Aberto, Em Análise ou Resolvido.");
    }
    public override void Write(Utf8JsonWriter writer, StatusChamado value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Texto());
}

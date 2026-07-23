using System.Text.Json.Serialization;

namespace GenericDataParser.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayloadType
{
    CSV,
    INTERNAL_JSON
}

using System.Text.Json;
using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

public class InternalJsonParser : IDataParser
{
    public PayloadType SupportedType => PayloadType.INTERNAL_JSON;

    private static readonly JsonSerializerOptions _jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public IEnumerable<object> Parse(string rawContent)
    {
        using var document = JsonDocument.Parse(rawContent);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<IEnumerable<object>>(rawContent, _jsonOptions)
                    ?? Enumerable.Empty<object>();
        }

        
        var singleObject = JsonSerializer.Deserialize<object>(rawContent, _jsonOptions);
        return singleObject != null ? [singleObject] : Enumerable.Empty<object>();
    }
}
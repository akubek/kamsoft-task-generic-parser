using System.Text.Json;
using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

/// <summary>
/// Parses INTERNAL_JSON payloads into a list of objects.
/// </summary>
public class InternalJsonParser : IDataParser
{
    /// <summary>
    /// Gets the payload type supported by this parser.
    /// </summary>
    public PayloadType SupportedType => PayloadType.INTERNAL_JSON;

    private static readonly JsonDocumentOptions _jsonDocumentOptions = new ()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip,
    };

    private static readonly JsonSerializerOptions _jsonOptions = new ()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Parses JSON content as an array or wraps a single object into a list.
    /// </summary>
    /// <param name="rawContent">Decoded JSON text content.</param>
    /// <returns>Parsed objects.</returns>
    /// <exception cref="JsonException">Thrown when content is not valid JSON.</exception>
    public IEnumerable<object> Parse(string rawContent)
    {
        using var document = JsonDocument.Parse(rawContent, _jsonDocumentOptions);

        if (document.RootElement.ValueKind == JsonValueKind.Array)
        {
            return JsonSerializer.Deserialize<IEnumerable<object>>(rawContent, _jsonOptions)
                    ?? Enumerable.Empty<object>();
        }

        
        var singleObject = JsonSerializer.Deserialize<object>(rawContent, _jsonOptions);
        return singleObject != null ? [singleObject] : Enumerable.Empty<object>();
    }
}
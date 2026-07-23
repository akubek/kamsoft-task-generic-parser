namespace GenericDataParser.Api.Models;

public record ParseResultResponse(
    string Status,
    string SourceType,
    int ProcessedItemsCount,
    IEnumerable Data
)

namespace GenericDataParser.Api.Models;

public record PayloadRequest(
    PayloadType Type,
    string Content
);

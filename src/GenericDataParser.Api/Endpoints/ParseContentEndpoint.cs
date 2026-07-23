using System.Text;
using System.Text.Json;
using CsvHelper;
using GenericDataParser.Api.Models;
using GenericDataParser.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenericDataParser.Api.Endpoints;

/// <summary>
/// Maps the content parsing endpoint.
/// </summary>
public static class ParseContentEndpoint
{
    /// <summary>
    /// Registers the POST /api/v1/parse-content endpoint.
    /// </summary>
    /// <param name="app">Application endpoint route builder.</param>
    /// <returns>Configured route handler builder.</returns>
    public static RouteHandlerBuilder MapParseContentEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPost("/api/v1/parse-content", HandleAsync)
            .WithSummary("Parses Base64-encoded CSV or INTERNAL_JSON payloads.")
            .WithDescription("Accepts application/json with payload type and Base64 content, decodes the content, parses it, and returns normalized JSON.")
            .Produces<ParseResultResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static IResult HandleAsync(
        [FromBody] PayloadRequest request,
        [FromServices] IParserFactory parserFactory,
        [FromServices] ILogger<Program> logger)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            logger.LogWarning("Rejected empty content for payload type {PayloadType}.", request.Type);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "Content cannot be empty",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var base64Bytes = Convert.FromBase64String(request.Content);
            var decodedContent = Encoding.UTF8.GetString(base64Bytes);

            var parser = parserFactory.GetParser(request.Type);
            var parsedData = parser.Parse(decodedContent).ToList();

            return Results.Ok(new ParseResultResponse(
                Status: "Success",
                SourceType: request.Type.ToString(),
                ProcessedItemsCount: parsedData.Count,
                Data: parsedData));
        }
        catch (FormatException ex)
        {
            logger.LogWarning(ex, "Rejected invalid Base64 content for payload type {PayloadType}.", request.Type);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Base64 decoding error",
                Detail = "Content is not a valid Base64 string",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (NotSupportedException ex)
        {
            logger.LogWarning(ex, "Rejected unsupported payload type {PayloadType}.", request.Type);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Unsupported payload type",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Rejected malformed JSON payload for payload type {PayloadType}.", request.Type);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "JSON Parsing Error",
                Detail = $"Failed to parse the decoded JSON content. Reason: {ex.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (CsvHelperException ex)
        {
            logger.LogWarning(ex, "Rejected malformed CSV payload for payload type {PayloadType}.", request.Type);
            return Results.BadRequest(new ProblemDetails
            {
                Title = "CSV Parsing Error",
                Detail = $"Failed to parse the decoded CSV content. Reason: {ex.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}

using System.Text;
using System.Text.Json;
using CsvHelper;
using GenericDataParser.Api.Models;
using GenericDataParser.Api.Services;
using Microsoft.AspNetCore.Mvc;
using CsvParser = GenericDataParser.Api.Services.CsvParser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDataParser>(sp => new CsvParser(CsvParserMode.Strict));
builder.Services.AddSingleton<IDataParser, InternalJsonParser>();

builder.Services.AddSingleton<IParserFactory, ParserFactory>();

var app = builder.Build();

// api endpoint for content parsing
app.MapPost("/api/v1/parse-content", (
    [FromBody] PayloadRequest request,
    [FromServices] IParserFactory parserFactory) =>
{
    if (string.IsNullOrWhiteSpace(request.Content))
    {
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

        var response = new ParseResultResponse(
            Status: "Success",
            SourceType: request.Type.ToString(),
            ProcessedItemsCount: parsedData.Count,
            Data: parsedData
        );

        return Results.Ok(response);
    }
    catch (FormatException)
    {
        return Results.BadRequest(new ProblemDetails
        {
            Title = "Base64 decoding error",
            Detail = "Content is not a valid Base64 string",
            Status = StatusCodes.Status400BadRequest
        });
    }
    catch (NotSupportedException ex)
    {
        return Results.BadRequest(new ProblemDetails
        {
            Title = "Unsupported payload type",
            Detail = ex.Message,
            Status = StatusCodes.Status400BadRequest
        });
    }
    catch (JsonException ex)
    {
        return Results.BadRequest(new ProblemDetails 
        { 
            Title = "JSON Parsing Error", 
            Detail = $"Failed to parse the decoded JSON content. Reason: {ex.Message}",
            Status = StatusCodes.Status400BadRequest
        });
    }
    catch (CsvHelperException ex)
    {
        return Results.BadRequest(new ProblemDetails 
        { 
            Title = "CSV Parsing Error", 
            Detail = $"Failed to parse the decoded CSV content. Reason: {ex.Message}",
            Status = StatusCodes.Status400BadRequest
        });
    }
});

app.Run();

public partial class Program
{
}

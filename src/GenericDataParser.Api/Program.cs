using GenericDataParser.Api.Services;
using GenericDataParser.Api.Endpoints;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IDataParser>(sp => new CsvParser(CsvParserMode.Strict));
builder.Services.AddSingleton<IDataParser, InternalJsonParser>();

builder.Services.AddSingleton<IParserFactory, ParserFactory>();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;

        logger.LogError(exception, "Unhandled exception while processing request.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Unexpected error",
            Detail = "An unexpected error occurred while processing the request.",
            Status = StatusCodes.Status500InternalServerError
        });
    });
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapParseContentEndpoint();

app.Run();

public partial class Program
{
}

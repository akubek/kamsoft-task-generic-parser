var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// api endpoint for content parsing
app.MapPost("/api/v1/parse-content", () => 
{
    return Results.Ok(new { message = "Endpoint ready" });
});

app.Run();

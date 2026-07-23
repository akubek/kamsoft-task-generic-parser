using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GenericDataParser.Api.Tests.Integration;

public class ParseContentEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ParseContentEndpointIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ParseContent_WhenCsvPayloadIsValid_ShouldReturnSuccessResponse()
    {
        var encodedCsv = EncodeBase64("Id,Name\n1,Artur\n2,Test");
        var payload = new PayloadRequest(PayloadType.CSV, encodedCsv);

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = responseJson.RootElement;

        Assert.Equal("Success", root.GetProperty("status").GetString());
        Assert.Equal("CSV", root.GetProperty("sourceType").GetString());
        Assert.Equal(2, root.GetProperty("processedItemsCount").GetInt32());
        var data = root.GetProperty("data");
        Assert.Equal(2, data.GetArrayLength());

        var firstRow = data[0];
        var secondRow = data[1];

        Assert.Equal("1", firstRow.GetProperty("Id").GetString());
        Assert.Equal("Artur", firstRow.GetProperty("Name").GetString());
        Assert.Equal("2", secondRow.GetProperty("Id").GetString());
        Assert.Equal("Test", secondRow.GetProperty("Name").GetString());
    }

    [Fact]
    public async Task ParseContent_WhenInternalJsonPayloadIsValid_ShouldReturnSuccessResponse()
    {
        var encodedJson = EncodeBase64("[{\"Id\":1,\"Author\":\"Artur\"}]");
        var payload = new PayloadRequest(PayloadType.INTERNAL_JSON, encodedJson);

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var responseJson = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = responseJson.RootElement;

        Assert.Equal("Success", root.GetProperty("status").GetString());
        Assert.Equal("INTERNAL_JSON", root.GetProperty("sourceType").GetString());
        Assert.Equal(1, root.GetProperty("processedItemsCount").GetInt32());
        var data = root.GetProperty("data");
        Assert.Equal(1, data.GetArrayLength());

        var firstRow = data[0];
        Assert.Equal(1, firstRow.GetProperty("Id").GetInt32());
        Assert.Equal("Artur", firstRow.GetProperty("Author").GetString());
    }

    [Fact]
    public async Task ParseContent_WhenContentIsWhitespaceOnly_ShouldReturnBadRequest()
    {
        var payload = new PayloadRequest(PayloadType.CSV, "   ");

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Invalid request", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenContentIsNotBase64_ShouldReturnBadRequest()
    {
        var payload = new PayloadRequest(PayloadType.CSV, "not-base64");

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Base64 decoding error", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenPayloadTypeIsUnsupported_ShouldReturnBadRequest()
    {
        var requestBody = "{\"type\":999,\"content\":\"" + EncodeBase64("Id,Name\\n1,Artur") + "\"}";
        using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/parse-content", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Unsupported payload type", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenStrictCsvHasMissingField_ShouldReturnBadRequest()
    {
        var encodedCsv = EncodeBase64("Id,Name\n1");
        var payload = new PayloadRequest(PayloadType.CSV, encodedCsv);

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("CSV Parsing Error", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenDecodedJsonIsInvalid_ShouldReturnBadRequest()
    {
        var encodedInvalidJson = EncodeBase64("{\"Id\":1,");
        var payload = new PayloadRequest(PayloadType.INTERNAL_JSON, encodedInvalidJson);

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("JSON Parsing Error", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenCsvHeaderHasDuplicateColumns_ShouldReturnBadRequest()
    {
        var encodedCsv = EncodeBase64("Id,Id\n1,2");
        var payload = new PayloadRequest(PayloadType.CSV, encodedCsv);

        var response = await _client.PostAsJsonAsync("/api/v1/parse-content", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("CSV Parsing Error", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenRequestBodyIsMissingContent_ShouldReturnBadRequest()
    {
        const string requestBody = "{\"type\":0}";
        using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/parse-content", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Invalid request", problem.Title);
    }

    [Fact]
    public async Task ParseContent_WhenContentTypeIsTextPlain_ShouldReturnUnsupportedMediaType()
    {
        using var content = new StringContent("plain text", Encoding.UTF8, "text/plain");

        var response = await _client.PostAsync("/api/v1/parse-content", content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    private static string EncodeBase64(string content)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
    }
}

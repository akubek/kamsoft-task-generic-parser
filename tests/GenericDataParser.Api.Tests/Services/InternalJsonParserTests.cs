using System.Text.Json;

namespace GenericDataParser.Api.Tests.Services;

public class InternalJsonParserTests
{
    private readonly InternalJsonParser _parser;

    public InternalJsonParserTests()
    {
        _parser = new InternalJsonParser();
    }

    [Fact]
    public void SupportedType_ShouldBe_InternalJson()
    {
        Assert.Equal(PayloadType.INTERNAL_JSON, _parser.SupportedType);
    }

    [Fact]
    public void Parse_WhenGivenJsonArray_ShouldReturnListOfObjects()
    {
        var jsonContent = """[{"Id": 1, "Author": "Artur Kubek"}, {"Id": 2, "Author": "Test Name"}]""";

        var result = _parser.Parse(jsonContent).ToList();

        var firstItem = (JsonElement)result[0];
        var secondItem = (JsonElement)result[1];

        Assert.Equal(2, result.Count);
        Assert.Equal(1, firstItem.GetProperty("Id").GetInt32());
        Assert.Equal("Artur Kubek", firstItem.GetProperty("Author").GetString());
        Assert.Equal(2, secondItem.GetProperty("Id").GetInt32());
        Assert.Equal("Test Name", secondItem.GetProperty("Author").GetString());
    }

    [Fact]
    public void Parse_WhenGivenSingleJsonObject_ShouldWrapInList()
    {
        var jsonContent = """{"Id": 1, "Author": "Artur Kubek"}""";

        var result = _parser.Parse(jsonContent).ToList();

        var firstItem = (JsonElement)result[0];

        Assert.Single(result);
        Assert.Equal(1, firstItem.GetProperty("Id").GetInt32());
        Assert.Equal("Artur Kubek", firstItem.GetProperty("Author").GetString());
    }

    [Fact]
    public void Parse_WhenGivenInvalidJson_ShouldThrowJsonException()
    {
        var invalidJson = """{"Id": 1, "Author": "Artur Kubek" """;

        Assert.ThrowsAny<JsonException>(() => _parser.Parse(invalidJson));
    }

    [Fact]
    public void Parse_WhenGivenJsonWithTrailingCommas_ShouldReturnListOfObjects()
    {
        var jsonWithTrailingCommas = """[{"Id": 1, "Author": "Artur Kubek",},]""";

        var result = _parser.Parse(jsonWithTrailingCommas).ToList();

        var firstItem = (JsonElement)result[0];
        Assert.Single(result);
        Assert.Equal(1, firstItem.GetProperty("Id").GetInt32());
        Assert.Equal("Artur Kubek", firstItem.GetProperty("Author").GetString());
    }

    [Fact]
    public void Parse_WhenGivenJsonWithComments_ShouldReturnListOfObjects()
    {
        var jsonWithComments = """
            [
              // first item
              { "Id": 1, "Author": "Artur Kubek" }
            ]
            """;

        var result = _parser.Parse(jsonWithComments).ToList();

        var firstItem = (JsonElement)result[0];
        Assert.Single(result);
        Assert.Equal(1, firstItem.GetProperty("Id").GetInt32());
        Assert.Equal("Artur Kubek", firstItem.GetProperty("Author").GetString());
    }

    [Fact]
    public void Parse_WhenContentIsNull_ShouldThrowArgumentNullException()
    {
        string nullJson = null!;

        Assert.Throws<ArgumentNullException>(() => _parser.Parse(nullJson));
    }

    [Fact]
    public void Parse_WhenContentIsEmptyString_ShouldThrowJsonException()
    {
        var emptyJson = "";

        Assert.ThrowsAny<JsonException>(() => _parser.Parse(emptyJson));
    }

    [Fact]
    public void Parse_WhenContentIsWhitespaceOnly_ShouldThrowJsonException()
    {
        var whitespaceOnlyJson = "   \n\t  ";

        Assert.ThrowsAny<JsonException>(() => _parser.Parse(whitespaceOnlyJson));
    }

    [Fact]
    public void Parse_WhenRootIsJsonNull_ShouldReturnEmptyList()
    {
        var nullRootJson = "null";

        var result = _parser.Parse(nullRootJson).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_WhenPropertyNamesUseDifferentCase_ShouldParseSuccessfully()
    {
        var jsonWithDifferentCase = """[{"id": 1, "author": "Artur Kubek"}]""";

        var result = _parser.Parse(jsonWithDifferentCase).ToList();

        var firstItem = (JsonElement)result[0];
        Assert.Single(result);
        Assert.Equal(1, firstItem.GetProperty("id").GetInt32());
        Assert.Equal("Artur Kubek", firstItem.GetProperty("author").GetString());
    }
}
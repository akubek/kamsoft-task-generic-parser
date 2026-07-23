using CsvHelper;

namespace GenericDataParser.Api.Tests.Services;

public class CsvStrictParserTests
{
    private readonly CsvParser _parser;

    public CsvStrictParserTests()
    {
        _parser = new CsvParser(CsvParserMode.Strict);
    }

    [Fact]
    public void SupportedType_ShouldBe_Csv()
    {
        Assert.Equal(PayloadType.CSV, _parser.SupportedType);
    }

    [Fact]
    public void Parse_WhenGivenValidCsv_ShouldReturnListOfObjects()
    {
        var csvContent = "Id,Name\n1,Artur\n2,Test";

        var result = _parser.Parse(csvContent).ToList();

        Assert.Equal(2, result.Count);
        
        var firstItem = (IDictionary<string, object>)result[0];
        var secondItem = (IDictionary<string, object>)result[1];
        Assert.Equal("1", firstItem["Id"]);
        Assert.Equal("Artur", firstItem["Name"]);
        Assert.Equal("2", secondItem["Id"]);
        Assert.Equal("Test", secondItem["Name"]);
    }

    [Fact]
    public void Parse_WhenCsvHasBlankLinesAndSpaces_ShouldApplyConfiguration()
    {
        var noisyCsvContent = "Id, Name \n\n 1 , Artur \n 2 , Test ";

        var result = _parser.Parse(noisyCsvContent).ToList();

        Assert.Equal(2, result.Count); // ignores blank lines
        
        var firstItem = (IDictionary<string, object>)result[0];
        var secondItem = (IDictionary<string, object>)result[1];
        Assert.Equal("1", firstItem["Id"]);
        Assert.Equal("Artur", firstItem["Name"]);
        Assert.Equal("2", secondItem["Id"]);
        Assert.Equal("Test", secondItem["Name"]);
    }

    [Fact]
    public void Parse_WhenCsvHasOnlyHeaders_ShouldReturnEmptyList()
    {
        var headersOnlyCsv = "Id,Name,Age";

        var result = _parser.Parse(headersOnlyCsv).ToList();

        Assert.Empty(result);
    }

    [Fact]
    public void Parse_WhenRowHasFewerFieldsThanHeader_ShouldThrowMissingFieldException()
    {
        var csvWithMissingField = "Id,Name\n1";

        Assert.ThrowsAny<CsvHelperException>(() => _parser.Parse(csvWithMissingField));
    }

    [Fact]
    public void Parse_WhenRowHasMoreFieldsThanHeader_ShouldThrowInconsistentColumnCountException()
    {
        var csvWithExtraField = "Id,Name\n1,\"Artur,\", ExtraText";

        Assert.ThrowsAny<CsvHelperException>(() => _parser.Parse(csvWithExtraField));
    }

    [Fact]
    public void Parse_WhenContentIsEmptyString_ShouldThrowArgumentException()
    {
        var emptyCsv = "";

        Assert.Throws<ArgumentException>(() => _parser.Parse(emptyCsv));
    }

}
using CsvHelper;
namespace GenericDataParser.Api.Tests.Services;

public class CsvLaxParserTests
{
    private readonly CsvParser _parser;

    public CsvLaxParserTests()
    {
        _parser = new CsvParser(CsvParserMode.Lax);
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
    public void Parse_WhenRowHasFewerFieldsThanHeader_ShouldNotThrowAndShouldSetMissingValueToNull()
    {
        var csvWithMissingField = "Id,Name\n1";

        var result = _parser.Parse(csvWithMissingField).ToList();

        Assert.Single(result);
        var firstItem = (IDictionary<string, object>)result[0];
        Assert.Equal("1", firstItem["Id"]);
        // Missing field in lax mode maps to null, unlike an explicit empty value.
        Assert.Null(firstItem["Name"]);
    }

    [Fact]
    public void Parse_WhenRowHasMoreFieldsThanHeader_ShouldNotThrowAndShouldIgnoreExtraData()
    {
        var csvWithExtraField = "Id,Name\n1,\"Artur,\", ExtraText";

        var result = _parser.Parse(csvWithExtraField).ToList();

        Assert.Single(result);
        var firstItem = (IDictionary<string, object>)result[0];
        Assert.Equal("1", firstItem["Id"]);
        Assert.Equal("Artur,", firstItem["Name"]);
    }

    [Fact]
    public void Parse_WhenFieldIsEmptyButColumnExists_ShouldNotThrowAndShouldKeepEmptyValue()
    {
        var csvWithEmptyField = "Id,Name\n1,";

        var result = _parser.Parse(csvWithEmptyField).ToList();

        Assert.Single(result);
        var firstItem = (IDictionary<string, object>)result[0];
        Assert.Equal("1", firstItem["Id"]);
        // Existing column with no characters maps to empty string.
        Assert.Equal(string.Empty, firstItem["Name"]);
    }

    [Fact]
    public void Parse_WhenContentIsEmptyString_ShouldThrowArgumentException()
    {
        var emptyCsv = "";

        Assert.Throws<ArgumentException>(() => _parser.Parse(emptyCsv));
    }

    [Fact]
    public void Parse_WhenContentIsWhitespaceOnly_ShouldThrowArgumentException()
    {
        var whitespaceOnlyCsv = "   \n\t  ";

        Assert.Throws<ArgumentException>(() => _parser.Parse(whitespaceOnlyCsv));
    }

    [Fact]
    public void Parse_WhenRowsAreMixed_ShouldNotThrowAndShouldParseKnownColumns()
    {
        var mixedRowsCsv = "Id,Name\n1,Artur\n2\n3,Test,ExtraText";

        var result = _parser.Parse(mixedRowsCsv).ToList();

        Assert.Equal(3, result.Count);

        var firstItem = (IDictionary<string, object>)result[0];
        var secondItem = (IDictionary<string, object>)result[1];
        var thirdItem = (IDictionary<string, object>)result[2];

        Assert.Equal("1", firstItem["Id"]);
        Assert.Equal("Artur", firstItem["Name"]);

        Assert.Equal("2", secondItem["Id"]);
        Assert.Null(secondItem["Name"]);

        Assert.Equal("3", thirdItem["Id"]);
        Assert.Equal("Test", thirdItem["Name"]);
    }

    [Fact]
    public void Parse_WhenHeaderHasDuplicateColumns_ShouldThrowCsvHelperException()
    {
        var csvWithDuplicateHeaders = "Id,Id\n1,2";

        Assert.ThrowsAny<CsvHelperException>(() => _parser.Parse(csvWithDuplicateHeaders));
    }

}
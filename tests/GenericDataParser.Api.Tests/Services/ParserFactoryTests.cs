namespace GenericDataParser.Api.Tests.Services;

public class ParserFactoryTests
{
    private class MockCsvParser : IDataParser
    {
        public PayloadType SupportedType => PayloadType.CSV;
        public IEnumerable<object> Parse(string rawContent) => throw new NotImplementedException();
    }

    private class MockJsonParser : IDataParser
    {
        public PayloadType SupportedType => PayloadType.INTERNAL_JSON;
        public IEnumerable<object> Parse(string rawContent) => throw new NotImplementedException();
    }

    [Fact]
    public void GetParser_WhenParserExists_ReturnsCorrectParser()
    {
        var parsers = new List<IDataParser> { new MockCsvParser(), new MockJsonParser() };
        var factory = new ParserFactory(parsers);

        var parser = factory.GetParser(PayloadType.CSV);

        Assert.NotNull(parser);
        Assert.Equal(PayloadType.CSV, parser.SupportedType);
        Assert.IsType<MockCsvParser>(parser);
    }

    [Fact]
    public void GetParser_WhenParserDoesNotExist_ThrowsNotSupportedException()
    {
        var factory = new ParserFactory([]);

        var exception = Assert.Throws<NotSupportedException>(() => factory.GetParser(PayloadType.CSV));
        Assert.Contains("No parser found", exception.Message);
    }
}
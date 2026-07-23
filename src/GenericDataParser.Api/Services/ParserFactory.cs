using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

public class ParserFactory : IParserFactory
{
    private readonly IEnumerable<IDataParser> _parsers;

    public ParserFactory(IEnumerable<IDataParser> parsers)
    {
        _parsers = parsers;
    }

    public IDataParser GetParser(PayloadType type)
    {
        var parser = _parsers.FirstOrDefault(p => p.SupportedType == type);
        if (parser == null)
        {
            throw new NotSupportedException($"No parser found for payload type: {type}");
        }
        return parser;
    }
}
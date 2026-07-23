using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

/// <summary>
/// Resolves parser implementations for supported payload types.
/// </summary>
public class ParserFactory : IParserFactory
{
    private readonly IEnumerable<IDataParser> _parsers;

    /// <summary>
    /// Initializes factory with all registered parser implementations.
    /// </summary>
    /// <param name="parsers">Available parser implementations.</param>
    public ParserFactory(IEnumerable<IDataParser> parsers)
    {
        _parsers = parsers;
    }

    /// <summary>
    /// Returns parser matching the requested payload type.
    /// </summary>
    /// <param name="type">Requested payload type.</param>
    /// <returns>Matching parser implementation.</returns>
    /// <exception cref="NotSupportedException">Thrown when no parser is registered for the type.</exception>
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
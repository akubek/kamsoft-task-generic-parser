using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

public interface IDataParser
{
    PayloadType SupportedType { get; }
    IEnumerable<object> Parse(string rawContent);
}

using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

public interface IParserFactory
{
    IDataParser GetParser(PayloadType type);
}

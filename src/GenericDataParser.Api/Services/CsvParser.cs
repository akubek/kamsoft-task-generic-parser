using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

public class CsvParser : IDataParser
{
    public PayloadType SupportedType => PayloadType.CSV;

    public IEnumerable<object> Parse(string rawContent)
    {
        using var reader = new StringReader(rawContent);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true, 
            TrimOptions = TrimOptions.Trim,
        };

        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<dynamic>().ToList();
    }
}

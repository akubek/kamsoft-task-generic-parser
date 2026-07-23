using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

public class CsvParser : IDataParser
{
    private readonly CsvConfiguration _config;
    public PayloadType SupportedType => PayloadType.CSV;

    public CsvParser(CsvParserMode mode = CsvParserMode.Strict)
    {
        _config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            DetectColumnCountChanges = mode == CsvParserMode.Strict, 
        };

        if (mode == CsvParserMode.Strict)
        {
            _config.MissingFieldFound = args => throw new CsvHelperException(args.Context, $"Missing field in row {args.Context.Parser?.Row.ToString() ?? "unknown"}.");
            _config.BadDataFound = args => throw new CsvHelperException(args.Context, $"Corrupted data in row {args.Context.Parser?.Row.ToString() ?? "unknown"}.");
        }
    }

    public IEnumerable<object> Parse(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new ArgumentException("CSV content cannot be null or empty.", nameof(rawContent));
        }

        using var reader = new StringReader(rawContent);
        using var csv = new CsvReader(reader, _config);
        return csv.GetRecords<dynamic>().ToList();
    }
}

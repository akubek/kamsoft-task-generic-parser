using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using GenericDataParser.Api.Models;

namespace GenericDataParser.Api.Services;

/// <summary>
/// Parses CSV payloads into a list of dynamic objects.
/// </summary>
public class CsvParser : IDataParser
{
    private readonly CsvConfiguration _config;
    private readonly CsvParserMode _mode;
    /// <summary>
    /// Gets the payload type supported by this parser.
    /// </summary>
    public PayloadType SupportedType => PayloadType.CSV;

    /// <summary>
    /// Initializes a new CSV parser with the selected validation mode.
    /// </summary>
    /// <param name="mode">CSV parsing mode (strict or lax).</param>
    public CsvParser(CsvParserMode mode = CsvParserMode.Strict)
    {
        _mode = mode;
        _config = CreateConfiguration(mode);

        if (_mode == CsvParserMode.Strict)
        {
            ConfigureStrictValidation(_config);
        }
    }

    /// <summary>
    /// Parses CSV content and returns records as objects.
    /// </summary>
    /// <param name="rawContent">Decoded CSV text content.</param>
    /// <returns>Parsed records represented as objects.</returns>
    /// <exception cref="ArgumentException">Thrown when the content is null, empty, or whitespace.</exception>
    /// <exception cref="CsvHelperException">Thrown when CSV content is invalid.</exception>
    public IEnumerable<object> Parse(string rawContent)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            throw new ArgumentException("CSV content cannot be null or empty.", nameof(rawContent));
        }

        using var reader = new StringReader(rawContent);
        using var csv = new CsvReader(reader, _config);

        if (!csv.Read())
        {
            return Enumerable.Empty<object>();
        }

        csv.ReadHeader();
        ThrowIfHeaderHasDuplicates(csv);

        return csv.GetRecords<dynamic>().ToList();
    }

    private static CsvConfiguration CreateConfiguration(CsvParserMode mode)
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true,
            TrimOptions = TrimOptions.Trim,
            DetectColumnCountChanges = mode == CsvParserMode.Strict,
        };
    }

    private static void ConfigureStrictValidation(CsvConfiguration config)
    {
        config.MissingFieldFound = args =>
            throw new CsvHelperException(args.Context, $"Missing field in row {args.Context.Parser?.Row.ToString() ?? "unknown"}.");

        config.BadDataFound = args =>
            throw new CsvHelperException(args.Context, $"Corrupted data in row {args.Context.Parser?.Row.ToString() ?? "unknown"}.");
    }

    private static void ThrowIfHeaderHasDuplicates(CsvReader csv)
    {
        var headerRecord = csv.HeaderRecord;
        if (headerRecord == null || headerRecord.Length == 0)
        {
            return;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var columnName in headerRecord)
        {
            var normalizedName = columnName ?? string.Empty;
            if (!seen.Add(normalizedName))
            {
                duplicates.Add(normalizedName);
            }
        }

        if (duplicates.Count > 0)
        {
            var duplicateNames = string.Join(", ", duplicates.OrderBy(name => name, StringComparer.OrdinalIgnoreCase));
            throw new CsvHelperException(csv.Context, $"Duplicate column names are not allowed: {duplicateNames}.");
        }
    }
}

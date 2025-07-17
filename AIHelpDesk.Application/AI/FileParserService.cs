using CsvHelper;
using System.Globalization;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

public class FileParserService : IFileParserService
{
    public async Task<string> ParseAsync(Stream fileStream, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => ParsePdf(fileStream),
            ".txt" => await ParseTxtAsync(fileStream),
            ".csv" => await ParseCsvAsync(fileStream),
            _ => throw new NotSupportedException($"Formato non supportato: {ext}")
        };
    }

    private string ParsePdf(Stream stream)
    {
        using var pdf = PdfDocument.Open(stream);
        var text = new StringBuilder();

        foreach (Page page in pdf.GetPages())
        {
            text.AppendLine(page.Text);
        }

        return text.ToString();
    }

    private async Task<string> ParseTxtAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task<string> ParseCsvAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var text = new StringBuilder();

        await foreach (var record in csv.GetRecordsAsync<dynamic>())
        {
            foreach (var kv in record)
            {
                text.Append($"{kv.Key}: {kv.Value} | ");
            }
            text.AppendLine();
        }

        return text.ToString();
    }
}

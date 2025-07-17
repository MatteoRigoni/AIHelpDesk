public interface IFileParserService
{
    Task<string> ParseAsync(Stream fileStream, string fileName);
}

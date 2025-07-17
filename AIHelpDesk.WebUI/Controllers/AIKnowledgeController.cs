using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("upload/ai-docs")]
public class AIKnowledgeController : ControllerBase
{
    private readonly IFileParserService _fileParserService;
    private readonly IWebHostEnvironment _env;

    public AIKnowledgeController(IFileParserService fileParserService, IWebHostEnvironment env)
    {
        _fileParserService = fileParserService;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File mancante.");

        using var stream = file.OpenReadStream();
        var parsedText = await _fileParserService.ParseAsync(stream, file.FileName);

        var parsedDocsPath = Path.Combine(_env.ContentRootPath, "ParsedDocs");

        if (!Directory.Exists(parsedDocsPath))
            Directory.CreateDirectory(parsedDocsPath);

        // nome file .txt basato sul nome originale
        var fileName = Path.ChangeExtension(Path.GetFileName(file.FileName), ".txt");
        var outputPath = Path.Combine(parsedDocsPath, fileName);

        // scrivo il file in asincrono
        await System.IO.File.WriteAllTextAsync(outputPath, parsedText);

        return Ok(new { File = file.FileName, Characters = parsedText.Length });
    }
}

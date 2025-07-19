using AIHelpDesk.Application.AI;
using AIHelpDesk.Infrastructure.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("upload/ai-docs")]
public class AIKnowledgeController : ControllerBase
{
    private readonly IFileParserService _fileParserService;
    private readonly DocumentIndexService _docIndexService;
    private readonly IOptions<TenantInfoOptions> _tenantOptions;
    private readonly IWebHostEnvironment _env;

    public AIKnowledgeController(
       IFileParserService fileParserService,
       DocumentIndexService docIndexService,
       IOptions<TenantInfoOptions> tenantOptions,
       IWebHostEnvironment env)
    {
        _fileParserService = fileParserService;
        _docIndexService = docIndexService;
        _tenantOptions = tenantOptions;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File mancante.");

        // 1. Estrai e indicizza tutto in Qdrant via DocumentIndexService
        await _docIndexService.IndexDocumentAsync(_tenantOptions.Value.TenantName, file);

        // 2. (Opzionale) salva ancora il file di testo per audit/log
        var parsedDocsPath = Path.Combine(_env.ContentRootPath, "ParsedDocs");
        Directory.CreateDirectory(parsedDocsPath);
        var fileName = Path.ChangeExtension(Path.GetFileName(file.FileName), ".txt");
        var outputPath = Path.Combine(parsedDocsPath, fileName);
        var parsedText = await _fileParserService.ParseAsync(file.OpenReadStream(), file.FileName);
        await System.IO.File.WriteAllTextAsync(outputPath, parsedText);

        return Ok(new
        {
            File = file.FileName,
            Tenant = _tenantOptions.Value.TenantName,
            Chunks = "indicate number of chunks if you want",
            IndexedAt = DateTime.UtcNow
        });
    }
}

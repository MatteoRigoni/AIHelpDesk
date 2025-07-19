using AIHelpDesk.Application.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("upload/ai-docs")]
public class AIKnowledgeController : ControllerBase
{
    private readonly IFileParserService _fileParserService;
    private readonly DocumentIndexService _docIndexService;
    private readonly IWebHostEnvironment _env;

    public AIKnowledgeController(
       IFileParserService fileParserService,
       DocumentIndexService docIndexService,
       IWebHostEnvironment env)
    {
        _fileParserService = fileParserService;
        _docIndexService = docIndexService;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File mancante.");

        // Ottieni l’ID del tenant: adatta questo al tuo multi‐tenant setup
        // Esempio: potresti avere un claim "tenant_id", oppure usare un subdominio, ecc.
        var tenantId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? HttpContext.Request.Headers["X-Tenant-ID"].FirstOrDefault()
                       ?? "default";

        // 1. Estrai e indicizza tutto in Qdrant via DocumentIndexService
        await _docIndexService.IndexDocumentAsync(tenantId, file);

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
            Tenant = tenantId,
            Chunks = "indicate number of chunks if you want",
            IndexedAt = DateTime.UtcNow
        });
    }
}

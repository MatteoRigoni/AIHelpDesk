using AIHelpDesk.Application;
using AIHelpDesk.Application.AI;
using AIHelpDesk.Domain;
using AIHelpDesk.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Infrastructure
{
    public class DocumentService : IDocumentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly DocumentIndexService _indexService;
        private readonly IFileParserService _parser;
        private readonly ITextChunkerService _chunker;

        public DocumentService(
            ApplicationDbContext db,
            IWebHostEnvironment env,
            DocumentIndexService indexService,
            IFileParserService parser,
            ITextChunkerService chunker)
        {
            _db = db;
            _env = env;
            _indexService = indexService;
            _parser = parser;
            _chunker = chunker;
        }

        public async Task<UploadedDocument> RegisterAsync(string tenantId, IFormFile file)
        {
            // 1) Salva il file su disco
            var uploadDir = Path.Combine(_env.ContentRootPath, "UploadedDocs");
            if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
            var filePath = Path.Combine(uploadDir, file.FileName);
            await using (var fs = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(fs);

            // 2) Crea il record DB
            var doc = new UploadedDocument
            {
                TenantId = tenantId,
                FileName = file.FileName,
                UploadedAt = DateTime.UtcNow,
                ChunkCount = 0,
                Status = "Pending",
            };
            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            // 3) Indicizzazione e aggiornamento stato
            try
            {
                await _indexService.IndexDocumentAsync(tenantId, file);

                // Calcola numero di chunk per report
                await using var parseStream = file.OpenReadStream();
                var text = await _parser.ParseAsync(parseStream, file.FileName);
                var chunks = _chunker.ChunkText(text).ToList();

                doc.ChunkCount = chunks.Count;
                doc.Status = "Indexed";
                doc.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                doc.Status = "Error";
                doc.ErrorMessage = ex.Message;
            }

            _db.Documents.Update(doc);
            await _db.SaveChangesAsync();
            return doc;
        }

        public async Task<IReadOnlyList<UploadedDocument>> ListAsync(string tenantId)
        {
            return await _db.Documents
                .Where(d => d.TenantId == tenantId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        public async Task DeleteAsync(int documentId)
        {
            var doc = await _db.Documents.FindAsync(documentId);
            if (doc == null) return;

            // Rimuovi file dal disco
            var path = Path.Combine(_env.ContentRootPath, "UploadedDocs", doc.FileName);
            if (File.Exists(path)) File.Delete(path);

            // Rimuovi record dal DB
            _db.Documents.Remove(doc);
            await _db.SaveChangesAsync();
        }

        public async Task ReindexAsync(int documentId)
        {
            var doc = await _db.Documents.FindAsync(documentId)
                      ?? throw new InvalidOperationException("Document not found");

            doc.Status = "Pending";
            doc.ErrorMessage = null;
            _db.Documents.Update(doc);
            await _db.SaveChangesAsync();

            // Ricrea IFormFile a partire dal file su disco
            var uploadDir = Path.Combine(_env.ContentRootPath, "UploadedDocs");
            var filePath = Path.Combine(uploadDir, doc.FileName);
            await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            IFormFile formFile = new FormFile(fs, 0, fs.Length, "file", doc.FileName);

            try
            {
                await _indexService.IndexDocumentAsync(doc.TenantId, formFile);

                // Ricalcola chunk count
                await using var parseStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var text = await _parser.ParseAsync(parseStream, doc.FileName);
                var chunks = _chunker.ChunkText(text).ToList();

                doc.ChunkCount = chunks.Count;
                doc.Status = "Indexed";
                doc.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                doc.Status = "Error";
                doc.ErrorMessage = ex.Message;
            }

            _db.Documents.Update(doc);
            await _db.SaveChangesAsync();
        }
    }
}

using AIHelpDesk.Infrastructure;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace AIHelpDesk.Application.AI
{
    public class DocumentIndexService
    {
        private readonly IFileParserService _parser;
        private readonly ITextChunkerService _chunker;
        private readonly IEmbeddingService _embedder;
        private readonly IVectorStoreService _store;

        public DocumentIndexService(
          IFileParserService parser,
          ITextChunkerService chunker,
          IEmbeddingService embedder,
          IVectorStoreService store)
        {
            _parser = parser;
            _chunker = chunker;
            _embedder = embedder;
            _store = store;
        }

        public async Task IndexDocumentAsync(string tenantId, IFormFile file)
        {
            var text = await _parser.ParseAsync(file.OpenReadStream(), file.FileName);
            var chunks = _chunker.ChunkText(text).ToList();
            var vecs = await _embedder.CreateEmbeddingsAsync(chunks);

            var items = chunks.Select((c, i) =>
                ($"{file.FileName}-{i}",          // id univoco: nomefile‑indice
                 vecs[i],                         // embedding
                 c));                             // testo

            await _store.StoreAsync(tenantId, items);
        }
    }
}

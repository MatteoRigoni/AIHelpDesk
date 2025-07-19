using AIHelpDesk.Infrastructure;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
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
                (_DeterministicGuid($"{file.FileName}-{i}").ToString(),  // id univoco: nomefile‑indice
                 $"{file.FileName}-{i}",          // filename più chunk index
                 vecs[i],                         // embedding
                 c));                             // testo

            await _store.StoreAsync(tenantId, items);
        }

        private Guid _DeterministicGuid(string input)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            Span<byte> g = stackalloc byte[16];
            hash.AsSpan(0, 16).CopyTo(g);

            g[6] = (byte)((g[6] & 0x0F) | 0x50);  // version 5
            g[8] = (byte)((g[8] & 0x3F) | 0x80);  // RFC 4122 variant

            return new Guid(g);
        }
    }
}

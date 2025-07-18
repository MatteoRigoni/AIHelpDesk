using AIHelpDesk.Infrastructure.Model;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIHelpDesk.Application.AI
{    public class OpenAiEmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingClient _embeddingClient;
        public OpenAiEmbeddingService(IOptionsSnapshot<OpenAiSettings> opts)
            => _embeddingClient = new EmbeddingClient(
                model: "text-embedding-3-small",
                apiKey: opts.Value.ApiKey);

        public async Task<IReadOnlyList<float[]>> CreateEmbeddingsAsync(IEnumerable<string> texts)
        {
            var inputs = texts?.ToList() ?? throw new ArgumentNullException(nameof(texts));
            if (inputs.Count == 0) return Array.Empty<float[]>();

            ClientResult<OpenAIEmbeddingCollection> result =
                await _embeddingClient.GenerateEmbeddingsAsync(inputs);

            OpenAIEmbeddingCollection embeddings = result.Value;

            return embeddings
                .Select(e => e.ToFloats().ToArray())
                .ToList();
        }
    }
}

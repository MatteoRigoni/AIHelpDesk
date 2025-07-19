using AIHelpDesk.Application.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace AIHelpDesk.Infrastructure
{
    public class ChatService : IChatService
    {
        private readonly IEmbeddingService _embedder;
        private readonly IVectorStoreService _vectorStore;
        private readonly ChatClient _chat;
        private readonly IChatLogService _log;

        public ChatService(
            IEmbeddingService embedder,
            IVectorStoreService vectorStore,
            ChatClient openAi,
            IChatLogService log)
        {
            _embedder = embedder;    // per generare embedding
            _vectorStore = vectorStore;  // per cercare chunk
            _chat = openAi;       // per chiamare GPT‑4
            _log = log;
        }

        public async Task<IReadOnlyList<Application.AI.ChatMessage>> AskAsync(string tenantId, string userQuestion, string userId)
        {
            // 1️. Embedding della domanda
            var qVec = (await _embedder.CreateEmbeddingsAsync(new[] { userQuestion }))[0];

            // 2️. Nearest‑neighbour search
            var chunks = await _vectorStore.QueryAsync(tenantId, qVec, topK: 5);

            // . Prompt RAG
            var prompt = new StringBuilder()
                .AppendLine("Usa solo queste informazioni per rispondere:")
                .AppendJoin('\n', chunks.Select(c => $"- {c.text.Trim().Replace('\n', ' ')}"))
                .AppendLine()
                .Append("Domanda: ").Append(userQuestion)
                .ToString();

            // 4️. Chat completion
            ChatCompletion completion = await _chat.CompleteChatAsync(
                 new SystemChatMessage(prompt),
                 new UserChatMessage(userQuestion));

            string aiAnswer = completion.Content[0].Text.Trim();     // v2 API :contentReference[oaicite:2]{index=2}

            await _log.LogAsync(tenantId, userId, userQuestion, aiAnswer);

            // 5️.Risposta per la UI
            return new[]
            {
                new Application.AI.ChatMessage("user",      userQuestion),
                new Application.AI.ChatMessage("assistant", aiAnswer)
            };
        }
    }
}
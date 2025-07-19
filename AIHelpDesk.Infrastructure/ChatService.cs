using AIHelpDesk.Application.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIHelpDesk.Infrastructure
{
    public class ChatService : IChatService
    {
        private readonly IEmbeddingService _embedder;
        private readonly IVectorStoreService _vectorStore;
        private readonly ChatClient _chat;
        private readonly IChatLogService _log;
        private readonly IPromptSettingsService _promptSvc;

        public ChatService(
            IEmbeddingService embedder,
            IVectorStoreService vectorStore,
            ChatClient openAi,
            IChatLogService log,
            IPromptSettingsService promptSvc)
        {
            _embedder = embedder;    // per generare embedding
            _vectorStore = vectorStore;  // per cercare chunk
            _chat = openAi;       // per chiamare GPT‑4
            _log = log;
            _promptSvc = promptSvc;
        }

        public async Task<IReadOnlyList<Application.AI.ChatMessage>> AskAsync(
            string tenantId,
            string userQuestion,
            string userId,
            double? temperature = null,
            double? topP = null,
            int? maxTokens = null)
        {
            var settings = await _promptSvc.GetAsync(tenantId);

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
            var messages = new List<OpenAI.Chat.ChatMessage>()
            {
                new SystemChatMessage("Sei un assistente virtuale che risponde a domande basate su documenti."),
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions()
            {
                Temperature = (float)settings.Temperature,
                TopP = (float)settings.TopP,
                MaxOutputTokenCount = settings.MaxTokens,     
            };

            ChatCompletion completion = await _chat.CompleteChatAsync(messages.ToArray(), options);

            string aiAnswer = completion.Content[0].Text.Trim();

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
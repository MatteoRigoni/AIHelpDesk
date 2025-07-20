using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application.AI
{
    public record ChatMessage(string Role, string Content);

    public interface IChatService
    {
        /// <summary>
        /// Riceve la domanda, esegue retrieval + GPT‑4 e restituisce la conversazione.
        /// </summary>
        Task<IReadOnlyList<Application.AI.ChatMessage>> AskAsync(
            string tenantId,
            string userQuestion,
            string userId,
            double? temperature = null,
            double? topP = null,
            int? maxTokens = null);
    }
}

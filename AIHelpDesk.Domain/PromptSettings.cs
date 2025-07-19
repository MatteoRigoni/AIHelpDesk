using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Domain
{
    public class PromptSettings
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = null!;
        public string SystemPrompt { get; set; } =
            "Sei un assistente HelpDesk interno, rispondi in modo chiaro.";
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 800;
        public double TopP { get; set; } = 1.0;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Domain
{
    public class ChatLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string TenantId { get; set; } = null!;
        public string UserMessage { get; set; } = null!;
        public string AIResponse { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}

using AIHelpDesk.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application.AI
{
    public interface IChatLogService
    {
        Task LogAsync(string tenantId, string userId, string userMsg, string aiResp);
        Task<IReadOnlyList<ChatLog>> GetHistoryAsync(string tenantId, string? userId = null, int limit = 100);
    }
}

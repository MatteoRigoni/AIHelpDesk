// AIHelpDesk.Infrastructure/AI/ChatLogService.cs
using AIHelpDesk.Application.AI;
using AIHelpDesk.Domain;
using AIHelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class ChatLogService : IChatLogService
{
    private readonly ApplicationDbContext _db;
    public ChatLogService(ApplicationDbContext db) => _db = db;

    public async Task LogAsync(string tenantId, string userId, string userMsg, string aiResp)
    {
        _db.ChatLogs.Add(new ChatLog
        {
            TenantId = tenantId,
            UserId = userId,
            UserMessage = userMsg,
            AIResponse = aiResp,
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ChatLog>> GetHistoryAsync(string tenantId, string? userId = null, int limit = 100)
    {
        var q = _db.ChatLogs
                   .Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrEmpty(userId))
            q = q.Where(x => x.UserId == userId);

        return await q
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}

using AIHelpDesk.Application.AI;
using AIHelpDesk.Domain;
using AIHelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Infrastructure
{
    public class PromptSettingsService : IPromptSettingsService
    {
        private readonly ApplicationDbContext _db;
        public PromptSettingsService(ApplicationDbContext db) => _db = db;

        public async Task<PromptSettings> GetAsync(string tenantId)
        {
            var settings = await _db.PromptSettings
                .FirstOrDefaultAsync(x => x.TenantId == tenantId);
            if (settings is null)
            {
                settings = new PromptSettings { TenantId = tenantId };
                _db.PromptSettings.Add(settings);
                await _db.SaveChangesAsync();
            }
            return settings;
        }

        public async Task SaveAsync(PromptSettings settings)
        {
            _db.PromptSettings.Update(settings);
            await _db.SaveChangesAsync();
        }
    }
}

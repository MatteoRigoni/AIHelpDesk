using AIHelpDesk.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application.AI
{
    public interface IPromptSettingsService
    {
        Task<PromptSettings> GetAsync(string tenantId);
        Task SaveAsync(PromptSettings settings);
    }
}

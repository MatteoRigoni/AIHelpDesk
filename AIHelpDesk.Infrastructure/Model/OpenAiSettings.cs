using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Infrastructure.Model
{
    public record OpenAiSettings
    {
        public string ApiKey { get; init; } = default!;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application.AI
{
    public interface IEmbeddingService
    {
        Task<IReadOnlyList<float[]>> CreateEmbeddingsAsync(IEnumerable<string> texts);
    }
}

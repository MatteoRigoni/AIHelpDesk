using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application.AI
{
    public class TextChunkerService : ITextChunkerService
    {
        const int ChunkSize = 4000, Overlap = 800;
        public IEnumerable<string> ChunkText(string text)
        {
            for (int pos = 0; pos < text.Length; pos += ChunkSize - Overlap)
            {
                int len = Math.Min(ChunkSize, text.Length - pos);
                yield return text.Substring(pos, len);
            }
        }
    }
}

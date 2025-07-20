using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Domain
{
    public class UploadedDocument
    {
        public int Id { get; set; }
        public string TenantId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public int ChunkCount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Indexed, Error
        public string? ErrorMessage { get; set; }
    }
}

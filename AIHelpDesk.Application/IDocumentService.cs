using AIHelpDesk.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelpDesk.Application
{
    public interface IDocumentService
    {
        Task<UploadedDocument> RegisterAsync(string tenantId, IFormFile file);
        Task<IReadOnlyList<UploadedDocument>> ListAsync(string tenantId);
        Task DeleteAsync(int documentId);
        Task ReindexAsync(int documentId); 
    }
}

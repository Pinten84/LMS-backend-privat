using LMS.Domain.Entities;
using LMS.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application;
public class DocumentService
{
            private readonly DocumentRepository _documentRepository;
            public DocumentService(DocumentRepository documentRepository) => _documentRepository = documentRepository;
            public Task<List<Document>> GetAllDocumentsAsync() => _documentRepository.GetAllAsync();
            public Task<Document?> GetDocumentByIdAsync(int id) => _documentRepository.GetByIdAsync(id);
            public Task<(List<Document> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search) => _documentRepository.GetPagedAsync(page, pageSize, search);
            public Task AddDocumentAsync(Document document) => _documentRepository.AddAsync(document);
            public Task UpdateDocumentAsync(Document document) => _documentRepository.UpdateAsync(document);
            public Task DeleteDocumentAsync(int id) => _documentRepository.DeleteAsync(id);
}

using Enterprise.Domain.Entities;

namespace Enterprise.Application.Common.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id);

    Task<List<Document>> GetByWorkspaceAsync(Guid workspaceId);

    Task AddAsync(Document document);

    Task UpdateAsync(Document document);
}
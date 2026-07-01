using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _context.Documents
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Document>> GetByWorkspaceAsync(Guid workspaceId)
    {
        return await _context.Documents
            .Where(x => x.WorkspaceId == workspaceId)
            .ToListAsync();
    }

    public async Task AddAsync(Document document)
    {
        await _context.Documents.AddAsync(document);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Document document)
    {
        _context.Documents.Update(document);

        await _context.SaveChangesAsync();
    }
}
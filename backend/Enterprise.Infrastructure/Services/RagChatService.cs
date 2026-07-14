using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using Enterprise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.Infrastructure.Services;

public class RagChatService : IChatService
{
    private readonly ApplicationDbContext _context;

    public RagChatService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> AskAsync(Guid workspaceId, string question)
    {
        var documents = await _context.Documents
            .Where(x => x.WorkspaceId == workspaceId)
            .Include(x => x.Chunks)
            .ToListAsync();

        if (documents.Count == 0)
        {
            return "No documents have been uploaded to this workspace yet.";
        }

        var relevantChunks = documents
            .SelectMany(x => x.Chunks)
            .Where(x => x.Content.Contains(question, StringComparison.OrdinalIgnoreCase) || question.Contains(x.Content, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        if (relevantChunks.Count == 0)
        {
            relevantChunks = documents
                .SelectMany(x => x.Chunks)
                .OrderByDescending(x => x.Content.Length)
                .Take(3)
                .ToList();
        }

        var contextText = string.Join(Environment.NewLine, relevantChunks.Select(x => x.Content));

        return $"Based on the uploaded documents, here is a concise answer: {contextText}";
    }
}

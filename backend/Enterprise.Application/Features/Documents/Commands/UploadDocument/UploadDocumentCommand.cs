using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using MediatR;

namespace Enterprise.Application.Features.Documents.Commands.UploadDocument;

public record UploadDocumentCommand(Guid WorkspaceId, string FileName, string ContentType, long FileSize, string StoragePath, string Content) : IRequest<UploadDocumentResult>;

public record UploadDocumentResult(Guid Id, string FileName, string ContentType, long FileSize, string StoragePath);

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, UploadDocumentResult>
{
    private readonly IDocumentRepository _documentRepository;

    public UploadDocumentCommandHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<UploadDocumentResult> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = new Document
        {
            WorkspaceId = request.WorkspaceId,
            FileName = request.FileName,
            OriginalFileName = request.FileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            StoragePath = request.StoragePath,
            IsProcessed = true
        };

        await _documentRepository.AddAsync(document);

        var chunks = SplitTextIntoChunks(request.Content, 1200);

        for (var index = 0; index < chunks.Count; index++)
        {
            var chunk = new DocumentChunk
            {
                DocumentId = document.Id,
                ChunkIndex = index,
                PageNumber = index + 1,
                Content = chunks[index]
            };

            document.Chunks.Add(chunk);
        }

        await _documentRepository.UpdateAsync(document);

        return new UploadDocumentResult(document.Id, document.FileName, document.ContentType, document.FileSize, document.StoragePath);
    }

    private static List<string> SplitTextIntoChunks(string text, int chunkSize)
    {
        var normalized = text.Replace("\r\n", "\n").Trim();
        var chunks = new List<string>();

        for (var index = 0; index < normalized.Length; index += chunkSize)
        {
            var length = Math.Min(chunkSize, normalized.Length - index);
            chunks.Add(normalized.Substring(index, length));
        }

        return chunks;
    }
}

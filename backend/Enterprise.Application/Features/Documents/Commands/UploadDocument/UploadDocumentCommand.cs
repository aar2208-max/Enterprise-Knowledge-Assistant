using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using MediatR;

namespace Enterprise.Application.Features.Documents.Commands.UploadDocument;

public record UploadDocumentCommand(Guid WorkspaceId, string FileName, string ContentType, long FileSize, string StoragePath) : IRequest<UploadDocumentResult>;

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

        var chunk = new DocumentChunk
        {
            DocumentId = document.Id,
            ChunkIndex = 0,
            PageNumber = 1,
            Content = $"Document '{request.FileName}' uploaded to workspace {request.WorkspaceId}."
        };

        document.Chunks.Add(chunk);
        await _documentRepository.UpdateAsync(document);

        return new UploadDocumentResult(document.Id, document.FileName, document.ContentType, document.FileSize, document.StoragePath);
    }
}

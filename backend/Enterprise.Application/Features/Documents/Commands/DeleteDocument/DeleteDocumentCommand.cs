using Enterprise.Application.Common.Interfaces;
using MediatR;

namespace Enterprise.Application.Features.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand(Guid DocumentId, Guid RequestedBy) : IRequest<bool>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public DeleteDocumentCommandHandler(IDocumentRepository documentRepository, IWorkspaceRepository workspaceRepository)
    {
        _documentRepository = documentRepository;
        _workspaceRepository = workspaceRepository;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId);

        if (document is null)
        {
            return false;
        }

        var workspace = await _workspaceRepository.GetByIdAsync(document.WorkspaceId);
        if (workspace is null || workspace.CreatedBy != request.RequestedBy)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(document.StoragePath) && File.Exists(document.StoragePath))
        {
            try
            {
                File.Delete(document.StoragePath);
            }
            catch
            {
                // Ignore file deletion failures; database cleanup should still proceed.
            }
        }

        await _documentRepository.DeleteAsync(request.DocumentId);
        return true;
    }
}

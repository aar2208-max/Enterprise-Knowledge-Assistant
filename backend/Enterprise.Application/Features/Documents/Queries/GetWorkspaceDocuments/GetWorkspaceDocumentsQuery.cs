using Enterprise.Application.Common.Interfaces;
using MediatR;

namespace Enterprise.Application.Features.Documents.Queries.GetWorkspaceDocuments;

public record GetWorkspaceDocumentsQuery(Guid WorkspaceId, Guid RequestedBy) : IRequest<GetWorkspaceDocumentsResult>;

public record WorkspaceDocumentDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAtUtc);

public record GetWorkspaceDocumentsResult(List<WorkspaceDocumentDto> Documents);

public class GetWorkspaceDocumentsQueryHandler : IRequestHandler<GetWorkspaceDocumentsQuery, GetWorkspaceDocumentsResult>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IWorkspaceRepository _workspaceRepository;

    public GetWorkspaceDocumentsQueryHandler(IDocumentRepository documentRepository, IWorkspaceRepository workspaceRepository)
    {
        _documentRepository = documentRepository;
        _workspaceRepository = workspaceRepository;
    }

    public async Task<GetWorkspaceDocumentsResult> Handle(GetWorkspaceDocumentsQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);

        if (workspace is null || workspace.CreatedBy != request.RequestedBy)
        {
            return new GetWorkspaceDocumentsResult(new List<WorkspaceDocumentDto>());
        }

        var documents = await _documentRepository.GetByWorkspaceAsync(request.WorkspaceId);

        var documentDtos = documents.Select(document => new WorkspaceDocumentDto(
            document.Id,
            document.FileName,
            document.OriginalFileName,
            document.ContentType,
            document.FileSize,
            document.CreatedAtUtc)).ToList();

        return new GetWorkspaceDocumentsResult(documentDtos);
    }
}

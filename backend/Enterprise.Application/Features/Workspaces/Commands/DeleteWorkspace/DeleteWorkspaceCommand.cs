using Enterprise.Application.Common.Interfaces;
using MediatR;

namespace Enterprise.Application.Features.Workspaces.Commands.DeleteWorkspace;

public record DeleteWorkspaceCommand(Guid WorkspaceId, Guid RequestedBy) : IRequest<bool>;

public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, bool>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public DeleteWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<bool> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);
        if (workspace is null || workspace.CreatedBy != request.RequestedBy)
        {
            return false;
        }

        await _workspaceRepository.DeleteAsync(request.WorkspaceId);
        return true;
    }
}

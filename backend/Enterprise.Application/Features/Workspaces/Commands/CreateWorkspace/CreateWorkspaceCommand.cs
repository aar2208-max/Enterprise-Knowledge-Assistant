using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using MediatR;

namespace Enterprise.Application.Features.Workspaces.Commands.CreateWorkspace;

public record CreateWorkspaceCommand(string Name, string Description, Guid CreatedBy) : IRequest<CreateWorkspaceResult>;

public record CreateWorkspaceResult(Guid Id, string Name, string Description);

public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, CreateWorkspaceResult>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public CreateWorkspaceCommandHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<CreateWorkspaceResult> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        var workspace = new Workspace
        {
            Name = request.Name,
            Description = request.Description,
            CreatedBy = request.CreatedBy
        };

        await _workspaceRepository.AddAsync(workspace);

        return new CreateWorkspaceResult(workspace.Id, workspace.Name, workspace.Description);
    }
}

using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using MediatR;

namespace Enterprise.Application.Features.Workspaces.Queries.GetUserWorkspaces;

public record GetUserWorkspacesQuery(Guid CreatedBy) : IRequest<GetUserWorkspacesResult>;

public record GetUserWorkspacesResult(List<Workspace> Workspaces);

public class GetUserWorkspacesQueryHandler : IRequestHandler<GetUserWorkspacesQuery, GetUserWorkspacesResult>
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public GetUserWorkspacesQueryHandler(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<GetUserWorkspacesResult> Handle(GetUserWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await _workspaceRepository.GetByCreatedByAsync(request.CreatedBy);
        return new GetUserWorkspacesResult(workspaces);
    }
}

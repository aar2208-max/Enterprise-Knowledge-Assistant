using Enterprise.Application.Features.Workspaces.Commands.CreateWorkspace;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkspacesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkspaceRequest request)
    {
        var command = new CreateWorkspaceCommand(request.Name, request.Description);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record CreateWorkspaceRequest(string Name, string Description);

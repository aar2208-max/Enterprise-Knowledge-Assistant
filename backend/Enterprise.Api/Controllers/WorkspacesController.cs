using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Enterprise.Application.Features.Documents.Commands.DeleteDocument;
using Enterprise.Application.Features.Documents.Queries.GetWorkspaceDocuments;
using Enterprise.Application.Features.Workspaces.Commands.CreateWorkspace;
using Enterprise.Application.Features.Workspaces.Commands.DeleteWorkspace;
using Enterprise.Application.Features.Workspaces.Queries.GetUserWorkspaces;
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

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetUserWorkspacesQuery(userId.Value);
        var result = await _mediator.Send(query);
        return Ok(result.Workspaces);
    }

    [HttpGet("{workspaceId:guid}/documents")]
    public async Task<IActionResult> GetDocuments(Guid workspaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetWorkspaceDocumentsQuery(workspaceId, userId.Value);
        var result = await _mediator.Send(query);
        return Ok(result.Documents);
    }

    [HttpDelete("{workspaceId:guid}")]
    public async Task<IActionResult> DeleteWorkspace(Guid workspaceId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var workspace = await _mediator.Send(new GetUserWorkspacesQuery(userId.Value));
        var existing = workspace.Workspaces.FirstOrDefault(w => w.Id == workspaceId);

        if (existing is null)
        {
            return NotFound();
        }

        await _mediator.Send(new DeleteWorkspaceCommand(workspaceId, userId.Value));
        return NoContent();
    }

    [HttpDelete("{workspaceId:guid}/documents/{documentId:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid workspaceId, Guid documentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var query = new GetWorkspaceDocumentsQuery(workspaceId, userId.Value);
        var result = await _mediator.Send(query);
        if (result.Documents.All(d => d.Id != documentId))
        {
            return NotFound();
        }

        var command = new DeleteDocumentCommand(documentId, userId.Value);
        var deleted = await _mediator.Send(command);
        return deleted ? NoContent() : Forbid();
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public record CreateWorkspaceRequest(string Name, string Description);

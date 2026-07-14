using Enterprise.Application.Features.Chat.Commands.AskQuestion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] AskQuestionRequest request)
    {
        var command = new AskQuestionCommand(request.WorkspaceId, request.Question);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record AskQuestionRequest(Guid WorkspaceId, string Question);

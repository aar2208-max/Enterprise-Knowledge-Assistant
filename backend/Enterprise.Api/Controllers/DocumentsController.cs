using Enterprise.Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromBody] UploadDocumentRequest request)
    {
        var command = new UploadDocumentCommand(request.WorkspaceId, request.FileName, request.ContentType, request.FileSize, request.StoragePath);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

public record UploadDocumentRequest(Guid WorkspaceId, string FileName, string ContentType, long FileSize, string StoragePath);

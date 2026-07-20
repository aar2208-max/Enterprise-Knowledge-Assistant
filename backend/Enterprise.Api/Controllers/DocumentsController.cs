using DocumentFormat.OpenXml.Packaging;
using Enterprise.Application.Features.Documents.Commands.UploadDocument;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UglyToad.PdfPig;

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
    public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { error = "No file was uploaded." });
        }

        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(uploadsRoot);

        var workspaceFolder = Path.Combine(uploadsRoot, request.WorkspaceId.ToString());
        Directory.CreateDirectory(workspaceFolder);

        var fileName = Path.GetFileName(request.File.FileName);
        var savedFilePath = Path.Combine(workspaceFolder, $"{Guid.NewGuid()}_{fileName}");

        await using (var fileStream = new FileStream(savedFilePath, FileMode.Create))
        {
            await request.File.CopyToAsync(fileStream);
        }

        var extractedText = await ExtractTextFromFileAsync(savedFilePath);

        var command = new UploadDocumentCommand(
            request.WorkspaceId,
            fileName,
            request.File.ContentType ?? "application/octet-stream",
            request.File.Length,
            savedFilePath,
            extractedText);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    private static async Task<string> ExtractTextFromFileAsync(string savedFilePath)
    {
        var fileExtension = Path.GetExtension(savedFilePath).ToLowerInvariant();

        if (fileExtension == ".pdf")
        {
            return await ReadPdfTextAsync(savedFilePath);
        }

        if (fileExtension == ".docx")
        {
            return ReadDocxText(savedFilePath);
        }

        await using var stream = new FileStream(savedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static async Task<string> ReadPdfTextAsync(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            return string.Join("\n\n", document.GetPages().Select(page => page.Text).Where(text => !string.IsNullOrWhiteSpace(text)));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string ReadDocxText(string filePath)
    {
        try
        {
            using var doc = WordprocessingDocument.Open(filePath, false);
            return string.Join("\n\n", doc.MainDocumentPart?.Document.Body?.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
                .Select(paragraph => paragraph.InnerText)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList() ?? new List<string>());
        }
        catch
        {
            return string.Empty;
        }
    }
}

public class UploadDocumentRequest
{
    public Guid WorkspaceId { get; set; }

    public IFormFile? File { get; set; }
}

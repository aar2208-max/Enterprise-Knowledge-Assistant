using Enterprise.Domain.Common;

namespace Enterprise.Domain.Entities;

public class Document : AuditableEntity
{
    public Guid WorkspaceId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string StoragePath { get; set; } = string.Empty;

    public bool IsProcessed { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public ICollection<DocumentChunk> Chunks { get; set; }
        = new List<DocumentChunk>();
}
using Enterprise.Domain.Common;

namespace Enterprise.Domain.Entities;

public class Workspace : AuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; }
        = new List<Document>();
}
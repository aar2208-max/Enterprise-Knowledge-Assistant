using Enterprise.Domain.Common;

namespace Enterprise.Domain.Entities;

public class ChatSession : AuditableEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ICollection<ChatMessage> Messages { get; set; }
        = new List<ChatMessage>();
}
using Enterprise.Domain.Common;

namespace Enterprise.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatSessionId { get; set; }

    public bool IsUserMessage { get; set; }

    public string Content { get; set; } = string.Empty;

    public ChatSession ChatSession { get; set; } = null!;
}
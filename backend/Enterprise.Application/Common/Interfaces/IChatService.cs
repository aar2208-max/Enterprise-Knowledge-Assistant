namespace Enterprise.Application.Common.Interfaces;

public interface IChatService
{
    Task<string> AskAsync(Guid workspaceId, string question);
}

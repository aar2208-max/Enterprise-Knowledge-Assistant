using Enterprise.Application.Common.Interfaces;
using MediatR;

namespace Enterprise.Application.Features.Chat.Commands.AskQuestion;

public record AskQuestionCommand(Guid WorkspaceId, string Question) : IRequest<AskQuestionResult>;

public record AskQuestionResult(string Answer);

public class AskQuestionCommandHandler : IRequestHandler<AskQuestionCommand, AskQuestionResult>
{
    private readonly IChatService _chatService;

    public AskQuestionCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<AskQuestionResult> Handle(AskQuestionCommand request, CancellationToken cancellationToken)
    {
        var answer = await _chatService.AskAsync(request.WorkspaceId, request.Question);
        return new AskQuestionResult(answer);
    }
}

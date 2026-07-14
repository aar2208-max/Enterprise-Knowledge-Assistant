using Enterprise.Application.Common.Interfaces;
using Enterprise.Domain.Entities;
using Enterprise.Domain.Enums;
using MediatR;

namespace Enterprise.Application.Features.Authentication.Commands.RegisterUser;

public record RegisterUserCommand(string FirstName, string LastName, string Email, string Password) : IRequest<RegisterUserResult>;

public record RegisterUserResult(Guid Id, string FirstName, string LastName, string Email, string Role, string Token);

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new InvalidOperationException("A user with that email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.Member,
            IsActive = true
        };

        await _userRepository.AddAsync(user);

        var token = _jwtService.GenerateToken(user.Id, user.Email, user.Role.ToString());
        return new RegisterUserResult(user.Id, user.FirstName, user.LastName, user.Email, user.Role.ToString(), token);
    }
}

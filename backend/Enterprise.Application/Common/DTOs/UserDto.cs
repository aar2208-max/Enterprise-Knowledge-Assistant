using Enterprise.Domain.Enums;

namespace Enterprise.Application.Common.DTOs;

public class UserDto
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Email { get; set; } = "";

    public UserRole Role { get; set; }
}
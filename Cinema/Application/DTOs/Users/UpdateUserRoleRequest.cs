using Domain.Enums;

namespace Application.DTOs.Users;

public sealed class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}
using Domain.Enums;

namespace Application;

public class CurrentUserInfo
{
    public long UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public bool IsAuthenticated { get; init; }
    public UserRole Role { get; init; }

    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsCashier => Role == UserRole.Cashier;
    public bool IsCustomer => Role == UserRole.Customer;
    public bool IsStaff => IsCashier || IsAdmin;
}

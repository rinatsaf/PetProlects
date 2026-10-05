using Shared.Abstractions;

namespace IdentityService.Domain;

public sealed class User : Entity<Guid>
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    
    
    private User() : base(Guid.Empty)
    {
    }
    
    private User(Guid id, string email, string passwordHash, UserRole role)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role.ToString();
        CreatedAt = DateTime.UtcNow;
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
    }
    
    public static User Create(string email, string passwordHash, UserRole role = UserRole.Customer)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        return new User(Guid.NewGuid(), email.ToLowerInvariant().Trim(), passwordHash, role);
    }
}


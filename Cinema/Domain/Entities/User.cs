using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserInteraction> Interactions { get; set; } = new List<UserInteraction>();
    public ICollection<UserPreference> Preferences { get; set; } = new List<UserPreference>();
    public ICollection<MovieReview>  Reviews { get; set; } = new List<MovieReview>();
}


using Domain.Common;

namespace Domain.Entities;

public sealed class UserPreference : BaseEntity
{
    public long UserId { get; set; }
    public required User User { get; set; }

    public required string FeatureKey { get; set; }
    public decimal Weight { get; set; }
}
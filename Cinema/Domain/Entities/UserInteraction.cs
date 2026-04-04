using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class UserInteraction : BaseEntity
{
    public long UserId { get; set; }
    public required User User { get; set; } 

    public long MovieId { get; set; }
    public required Movie Movie { get; set; } 

    public InteractionType ActionType { get; set; }
    public decimal WeightDelta { get; set; }
}
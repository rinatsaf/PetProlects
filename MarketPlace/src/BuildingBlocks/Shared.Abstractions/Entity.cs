namespace Shared.Abstractions;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected set;}
    protected Entity(TId id) => Id = id;

    public bool Equals(Entity<TId>? other) =>
        other is not null && Id.Equals(other.Id);

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();
    
    public static bool operator ==(Entity<TId> left, Entity<TId> right) =>
        left?.Equals(right) ?? false;

    public static bool operator !=(Entity<TId> left, Entity<TId> right) =>
        !(left == right);
}

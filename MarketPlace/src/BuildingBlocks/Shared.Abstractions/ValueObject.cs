namespace Shared.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public bool Equals(ValueObject? other) =>
        other is not null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override bool Equals(object? obj) =>
        obj is ValueObject other && Equals(other);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((a, b) => a ^ b);

    public static bool operator ==(ValueObject left, ValueObject right) =>
        left?.Equals(right) ?? false;

    public static bool operator !=(ValueObject left, ValueObject right) =>
        !(left == right);
}
using Shared.Abstractions;

namespace ProductService.Domain.Shared;

public sealed class CategoryId : ValueObject
{
    public Guid Value { get; }
    
    private CategoryId(Guid value) => Value = value;

    public static CategoryId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Value cannot be empty", nameof(value));

        return new CategoryId(value);
    }
    
    public static CategoryId New() => new(Guid.NewGuid());
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

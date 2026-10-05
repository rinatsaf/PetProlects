using Shared.Abstractions;

namespace ProductService.Domain.Products;

public class ProductImage : ValueObject
{
    public string Url { get; }
    public string AltText { get; }
    public bool IsPrimary { get; }

    private ProductImage(string url, string altText, bool isPrimary)
    {
        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
    }

    public static ProductImage Create(string url, string altText, bool isPrimary)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Url cannot be empty", nameof(url));

        return new ProductImage(url, altText, isPrimary);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Url;
        yield return AltText;
        yield return IsPrimary;
    }
}
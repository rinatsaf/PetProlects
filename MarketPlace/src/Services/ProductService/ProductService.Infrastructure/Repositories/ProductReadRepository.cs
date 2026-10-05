using MongoDB.Driver;
using ProductService.Application.Common.Abstractions;
using ProductService.Domain.Products;
using ProductService.Infrastructure.Mappings;

namespace ProductService.Infrastructure.Repositories;

public sealed class ProductReadRepository : IProductReadRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductReadRepository(IMongoDatabase database)
    {
        ProductMongoMapping.Configure();
        _products = database.GetCollection<Product>("products");
    }
    
    public async Task<PagedResult<ProductReadModel>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.Empty;
        return await PaginateAsync(filter, page,  pageSize, ct);
    }

    public async Task<PagedResult<ProductReadModel>> SearchAsync(string term, int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.Regex(
            p => p.Name, new MongoDB.Bson.BsonRegularExpression(term, "i"));
        return await PaginateAsync(filter, page, pageSize, ct);
    }

    public async Task<PagedResult<ProductReadModel>> GetByCategoryAsync(Guid categoryId, int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.CategoryId, categoryId);
        return await PaginateAsync(filter, page, pageSize, ct);
    }

    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _products.Find(p => p.Id == id).FirstOrDefaultAsync(ct);
        return product is null ? null : MapToReadModel(product);
    }
    
    private async Task<PagedResult<ProductReadModel>> PaginateAsync(
        FilterDefinition<Product> filter, int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await _products.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _products
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ProductReadModel>(
            items.Select(MapToReadModel).ToList(),
            (int)totalCount,
            page,
            pageSize);
    }

    private static ProductReadModel MapToReadModel(Product p) => new(
        p.Id,
        p.Name,
        p.Price.Amount,
        p.Price.Currency,
        p.Images.FirstOrDefault(i => i.IsPrimary)?.Url,
        p.Status);
}
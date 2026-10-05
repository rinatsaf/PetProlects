using MongoDB.Driver;
using ProductService.Application.Common.Abstractions;
using ProductService.Domain.Products;
using ProductService.Infrastructure.Mappings;

namespace ProductService.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(IMongoDatabase database)
    {
        ProductMongoMapping.Configure();
        _products = database.GetCollection<Product>("products");
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _products.Find(p => p.Id == id).FirstOrDefaultAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await _products.InsertOneAsync(product, cancellationToken: ct);

    public async Task UpdateAsync(Product product, CancellationToken ct = default) =>
        await _products.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken: ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct = default) =>
        await _products.DeleteOneAsync(p => p.Id == id, ct);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) => 
        await _products.Find(p => p.Id == id).AnyAsync(ct);
}
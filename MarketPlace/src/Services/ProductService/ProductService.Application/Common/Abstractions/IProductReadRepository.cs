namespace ProductService.Application.Common.Abstractions;

public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);

public interface IProductReadRepository
{
    Task<PagedResult<ProductReadModel>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<ProductReadModel>> SearchAsync(string term, int page, int pageSize, CancellationToken ct = default);
    Task<PagedResult<ProductReadModel>> GetByCategoryAsync(Guid categoryId, int page, int pageSize, CancellationToken ct = default);
    Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
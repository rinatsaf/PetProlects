using MediatR;
using ProductService.Application.Common.Abstractions;

namespace ProductService.Application.Products.Queries.SearchProducts;

public sealed class SearchProductsHandler(IProductReadRepository readRepository)
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductReadModel>>
{
    private readonly IProductReadRepository _readRepository = readRepository;

    public async Task<PagedResult<ProductReadModel>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            return await _readRepository.SearchAsync(request.SearchTerm, request.Page, request.PageSize,
                cancellationToken);

        if (request.CategoryId.HasValue)
            return await _readRepository.GetByCategoryAsync(request.CategoryId.Value, request.Page, request.PageSize,
                cancellationToken);
        
        return await _readRepository.GetPagedAsync(request.Page, request.PageSize, cancellationToken);
    }
}
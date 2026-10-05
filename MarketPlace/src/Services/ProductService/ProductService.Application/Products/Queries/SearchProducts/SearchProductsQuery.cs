using MediatR;
using ProductService.Application.Common.Abstractions;

namespace ProductService.Application.Products.Queries.SearchProducts;

public sealed record SearchProductsQuery(string? SearchTerm, Guid? CategoryId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<ProductReadModel>>;



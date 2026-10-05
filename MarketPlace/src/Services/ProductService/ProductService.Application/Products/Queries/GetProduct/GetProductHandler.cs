using MediatR;
using ProductService.Application.Common.Abstractions;
using ProductService.Domain.Products;

namespace ProductService.Application.Products.Queries.GetProduct;

public sealed class GetProductHandler(IProductReadRepository readRepository)
    : IRequestHandler<GetProductQuery, ProductReadModel?>
{
    public async Task<ProductReadModel?> Handle(GetProductQuery request, CancellationToken ct) =>
        await readRepository.GetByIdAsync(request.Id, ct);
}
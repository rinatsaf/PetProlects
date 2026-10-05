using MediatR;
using ProductService.Application.Common.Abstractions;

namespace ProductService.Application.Products.Queries.GetProduct;

public sealed record GetProductQuery(Guid Id) : IRequest<ProductReadModel?>;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Products.Commands.CreateProduct;
using ProductService.Application.Products.Queries.GetProduct;
using ProductService.Application.Products.Queries.SearchProducts;

namespace ProductService.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        group.MapPost("/", async (CreateProductCommand command, [FromServices]IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"api/products/{id}", id);
        });
        
        group.MapGet("/{id:Guid}", async (Guid id, [FromServices]IMediator mediator) =>
        {
            var product = await mediator.Send(new GetProductQuery(id));
            return product is null ? Results.NotFound() : Results.Ok(product);
        });
        
        group.MapGet("/", async (
            [FromServices] IMediator mediator,
            string? search,
            Guid? categoryId,
            int page = 1,
            int pageSize = 20) =>
        {
            var query = new SearchProductsQuery(search, categoryId, page, pageSize);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        });
    }
}
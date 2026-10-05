using BasketService.Application.Commands.AddItemToBasket;
using BasketService.Application.Commands.CheckoutBasket;
using BasketService.Application.Commands.RemoveBasketItem;
using BasketService.Application.Commands.UpdateBasketItem;
using BasketService.Application.Queries.GetBasket;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Api.Endpoints;

public static class BasketEndpoints
{
    public static void AddBasketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/basket").WithTags("Baskets");

        group.MapGet("/", async ([FromServices] IMediator mediator, Guid userId) =>
        {
            var basket = await mediator.Send(new GetBasketQuery(userId));
            return basket is null ? Results.NotFound() : Results.Ok(basket);
        });
        
        group.MapPost("/items", async (
            [FromServices] IMediator mediator,
            AddItemToBasketCommand command) =>
        {
            await mediator.Send(command);
            return Results.Ok();
        });

        group.MapPut("/items", async (
            [FromServices] IMediator mediator,
            UpdateBasketItemCommand command) =>
        {
            await mediator.Send(command);
            return Results.Ok();
        });

        group.MapDelete("/items/{productId:guid}", async (
            [FromServices] IMediator mediator,
            Guid userId,
            Guid productId) =>
        {
            await mediator.Send(new RemoveBasketItemCommand(userId, productId));
            return Results.Ok();
        });

        group.MapPost("/checkout", async (
            [FromServices] IMediator mediator,
            Guid userId) =>
        {
            await mediator.Send(new CheckoutBasketCommand(userId));
            return Results.Ok();
        });
    }
}
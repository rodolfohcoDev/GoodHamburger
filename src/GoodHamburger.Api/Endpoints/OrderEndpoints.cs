using GoodHamburger.Application.Dtos;
using GoodHamburger.Application.Orders.Commands;
using GoodHamburger.Application.Orders.Queries;
using Mediator;

namespace GoodHamburger.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateOrderCommand(request.MenuItemIds);
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/orders/{result.Id}", result);
        })
        .WithTags("Orders")
        .WithOpenApi()
        .WithSummary("Cria um novo pedido");

        app.MapGet("/api/orders", async (
            IMediator mediator,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await mediator.Send(new ListOrdersQuery(page, pageSize), ct);
            return Results.Ok(result);
        })
        .WithTags("Orders")
        .WithOpenApi()
        .WithSummary("Lista pedidos com paginação");

        app.MapGet("/api/orders/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetOrderByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .WithTags("Orders")
        .WithOpenApi()
        .WithSummary("Retorna um pedido pelo ID");

        app.MapPut("/api/orders/{id:guid}", async (
            Guid id,
            UpdateOrderRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new UpdateOrderCommand(id, request.MenuItemIds);
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        })
        .WithTags("Orders")
        .WithOpenApi()
        .WithSummary("Atualiza um pedido existente");

        app.MapDelete("/api/orders/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteOrderCommand(id), ct);
            return Results.NoContent();
        })
        .WithTags("Orders")
        .WithOpenApi()
        .WithSummary("Remove um pedido");

        return app;
    }
}

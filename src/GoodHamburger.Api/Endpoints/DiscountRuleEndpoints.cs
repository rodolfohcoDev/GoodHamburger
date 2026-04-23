using GoodHamburger.Application.DiscountRules.Commands;
using GoodHamburger.Application.DiscountRules.Queries;
using GoodHamburger.Application.Dtos;
using Mediator;

namespace GoodHamburger.Api.Endpoints;

public static class DiscountRuleEndpoints
{
    public static IEndpointRouteBuilder MapDiscountRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/discount-rules").WithTags("DiscountRules").WithOpenApi();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDiscountRulesQuery(), ct);
            return Results.Ok(result);
        }).WithSummary("Lista todas as regras de desconto");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDiscountRuleByIdQuery(id), ct);
            return Results.Ok(result);
        }).WithSummary("Busca regra de desconto por ID");

        group.MapPost("/", async (CreateDiscountRuleRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new CreateDiscountRuleCommand(
                request.Name, request.Percent, request.MatchMode,
                request.RequiresSandwich, request.RequiresFries, request.RequiresDrink,
                request.RequiredMenuItemIds, request.Priority, request.IsActive,
                request.MinimumSubtotal, request.ValidFrom, request.ValidUntil);

            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/discount-rules/{result.Id}", result);
        }).WithSummary("Cria nova regra de desconto");

        group.MapPut("/{id:guid}", async (Guid id, UpdateDiscountRuleRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var command = new UpdateDiscountRuleCommand(
                id, request.Name, request.Percent, request.MatchMode,
                request.RequiresSandwich, request.RequiresFries, request.RequiresDrink,
                request.RequiredMenuItemIds, request.Priority, request.IsActive,
                request.MinimumSubtotal, request.ValidFrom, request.ValidUntil);

            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        }).WithSummary("Atualiza regra de desconto");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteDiscountRuleCommand(id), ct);
            return Results.NoContent();
        }).WithSummary("Remove regra de desconto");

        group.MapPost("/simulate", async (SimulateDiscountRequest request, IMediator mediator, CancellationToken ct) =>
        {
            var query = new SimulateDiscountQuery(request.MenuItemIds, request.AtUtc);
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        }).WithSummary("Simula desconto para uma lista de itens");

        return app;
    }
}

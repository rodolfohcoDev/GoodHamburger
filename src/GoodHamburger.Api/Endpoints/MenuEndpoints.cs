using GoodHamburger.Application.Dtos;
using GoodHamburger.Application.Menu.Commands;
using GoodHamburger.Application.Menu.Queries;
using Mediator;

namespace GoodHamburger.Api.Endpoints;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        // Public menu (active only)
        app.MapGet("/api/menu", async (IMediator mediator, CancellationToken ct) =>
        {
            var items = await mediator.Send(new GetMenuQuery(IncludeInactive: false), ct);
            return Results.Ok(items);
        })
        .WithTags("Menu")
        .WithOpenApi()
        .WithSummary("Retorna o cardápio ativo");

        // Admin: full list including inactive
        app.MapGet("/api/admin/menu", async (IMediator mediator, CancellationToken ct) =>
        {
            var items = await mediator.Send(new GetMenuQuery(IncludeInactive: true), ct);
            return Results.Ok(items);
        })
        .WithTags("Admin - Menu")
        .WithOpenApi()
        .WithSummary("Retorna todos os itens (incluindo inativos)");

        // Admin: get single item
        app.MapGet("/api/admin/menu/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var item = await mediator.Send(new GetMenuItemByIdQuery(id), ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        })
        .WithTags("Admin - Menu")
        .WithOpenApi()
        .WithSummary("Retorna um item pelo ID");

        // Admin: create
        app.MapPost("/api/admin/menu", async (CreateMenuItemRequest req, IMediator mediator, CancellationToken ct) =>
        {
            var cmd = new CreateMenuItemCommand(req.Code, req.Name, req.Price, req.Category);
            var item = await mediator.Send(cmd, ct);
            return Results.Created($"/api/admin/menu/{item.Id}", item);
        })
        .WithTags("Admin - Menu")
        .WithOpenApi()
        .WithSummary("Cria um novo item no cardápio");

        // Admin: update
        app.MapPut("/api/admin/menu/{id:guid}", async (Guid id, UpdateMenuItemRequest req, IMediator mediator, CancellationToken ct) =>
        {
            var cmd = new UpdateMenuItemCommand(id, req.Name, req.Price, req.Category, req.IsActive);
            var item = await mediator.Send(cmd, ct);
            return Results.Ok(item);
        })
        .WithTags("Admin - Menu")
        .WithOpenApi()
        .WithSummary("Atualiza um item do cardápio");

        // Admin: delete
        app.MapDelete("/api/admin/menu/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            await mediator.Send(new DeleteMenuItemCommand(id), ct);
            return Results.NoContent();
        })
        .WithTags("Admin - Menu")
        .WithOpenApi()
        .WithSummary("Remove um item do cardápio");

        return app;
    }
}


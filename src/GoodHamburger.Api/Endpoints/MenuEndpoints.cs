using GoodHamburger.Application.Menu.Queries;
using Mediator;

namespace GoodHamburger.Api.Endpoints;

public static class MenuEndpoints
{
    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/menu", async (IMediator mediator, CancellationToken ct) =>
        {
            var items = await mediator.Send(new GetMenuQuery(), ct);
            return Results.Ok(items);
        })
        .WithTags("Menu")
        .WithOpenApi()
        .WithSummary("Retorna o cardápio completo");

        return app;
    }
}

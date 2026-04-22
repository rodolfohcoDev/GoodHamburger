using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using Mediator;

namespace GoodHamburger.Application.Orders.Queries;

public sealed record ListOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<OrderResponse>>;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, PagedResult<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;

    public ListOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async ValueTask<PagedResult<OrderResponse>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _orderRepository.ListAsync(request.Page, request.PageSize, cancellationToken);
        var responses = items.Select(o => o.ToResponse()).ToList();
        return new PagedResult<OrderResponse>(responses, total, request.Page, request.PageSize);
    }
}

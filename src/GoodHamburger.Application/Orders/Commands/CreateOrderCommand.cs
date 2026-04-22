using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Services;
using Mediator;

namespace GoodHamburger.Application.Orders.Commands;

public sealed record CreateOrderCommand(List<Guid> MenuItemIds) : IRequest<OrderResponse>;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDiscountPolicy _discountPolicy;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMenuRepository menuRepository,
        IUnitOfWork unitOfWork,
        IDiscountPolicy discountPolicy)
    {
        _orderRepository = orderRepository;
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
        _discountPolicy = discountPolicy;
    }

    public async ValueTask<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var menuItems = await _menuRepository.GetByIdsAsync(request.MenuItemIds, cancellationToken);

        if (menuItems.Count != request.MenuItemIds.Count)
            throw new InvalidOrderException("Um ou mais itens do cardápio não foram encontrados.");

        var order = Order.Create();
        foreach (var item in menuItems)
            order.AddItem(item);

        order.Recalculate(_discountPolicy);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToResponse();
    }
}

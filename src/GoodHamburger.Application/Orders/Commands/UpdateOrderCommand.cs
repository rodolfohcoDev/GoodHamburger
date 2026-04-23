using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.Orders.Commands;

public sealed record UpdateOrderCommand(Guid OrderId, List<Guid> MenuItemIds) : IRequest<OrderResponse>;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncDiscountPolicy _discountPolicy;

    public UpdateOrderCommandHandler(
        IOrderRepository orderRepository,
        IMenuRepository menuRepository,
        IUnitOfWork unitOfWork,
        IAsyncDiscountPolicy discountPolicy)
    {
        _orderRepository = orderRepository;
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
        _discountPolicy = discountPolicy;
    }

    public async ValueTask<OrderResponse> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOrderException("Pedido não encontrado.", isNotFound: true);

        var menuItems = await _menuRepository.GetByIdsAsync(request.MenuItemIds, cancellationToken);

        if (menuItems.Count != request.MenuItemIds.Count)
            throw new InvalidOrderException("Um ou mais itens do cardápio não foram encontrados.");

        order.ReplaceItems(menuItems);
        var subtotal = order.Items.Sum(i => i.UnitPrice);
        var discount = await _discountPolicy.CalculateAsync(order.Items, subtotal, DateTime.UtcNow, cancellationToken);
        order.ApplyDiscount(discount.Percent, discount.AppliedRuleId, discount.AppliedRuleName);

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToResponse();
    }
}

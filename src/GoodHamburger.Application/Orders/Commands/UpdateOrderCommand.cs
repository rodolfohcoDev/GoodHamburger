using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Services;
using Mediator;

namespace GoodHamburger.Application.Orders.Commands;

public sealed record UpdateOrderCommand(Guid OrderId, List<Guid> MenuItemIds) : IRequest<OrderResponse>;

public sealed class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDiscountPolicy _discountPolicy;

    public UpdateOrderCommandHandler(
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

    public async ValueTask<OrderResponse> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOrderException("Pedido não encontrado.", isNotFound: true);

        var menuItems = await _menuRepository.GetByIdsAsync(request.MenuItemIds, cancellationToken);

        if (menuItems.Count != request.MenuItemIds.Count)
            throw new InvalidOrderException("Um ou mais itens do cardápio não foram encontrados.");

        order.ReplaceItems(menuItems);
        order.Recalculate(_discountPolicy);

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.ToResponse();
    }
}

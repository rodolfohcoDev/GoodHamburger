using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.Orders.Commands;

public sealed record DeleteOrderCommand(Guid OrderId) : IRequest;

public sealed class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Unit> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOrderException("Pedido não encontrado.", isNotFound: true);

        _orderRepository.Delete(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

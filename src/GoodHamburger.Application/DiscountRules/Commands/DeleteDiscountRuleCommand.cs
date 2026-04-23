using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.DiscountRules.Commands;

public sealed record DeleteDiscountRuleCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteDiscountRuleCommandHandler : IRequestHandler<DeleteDiscountRuleCommand, Unit>
{
    private readonly IDiscountRepository _discountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDiscountRuleCommandHandler(IDiscountRepository discountRepository, IUnitOfWork unitOfWork)
    {
        _discountRepository = discountRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Unit> Handle(DeleteDiscountRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _discountRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidDiscountRuleException("Regra de desconto não encontrada.", isNotFound: true);

        _discountRepository.Delete(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

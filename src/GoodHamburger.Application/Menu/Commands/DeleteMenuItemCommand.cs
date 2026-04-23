using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.Menu.Commands;

public sealed record DeleteMenuItemCommand(Guid Id) : IRequest;

public sealed class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMenuItemCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<Unit> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _menuRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOrderException("Item do cardápio não encontrado.", isNotFound: true);

        _menuRepository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

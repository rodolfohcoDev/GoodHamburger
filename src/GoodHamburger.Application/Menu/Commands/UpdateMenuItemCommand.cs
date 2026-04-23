using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using Mediator;

namespace GoodHamburger.Application.Menu.Commands;

public sealed record UpdateMenuItemCommand(
    Guid Id,
    string Name,
    decimal Price,
    ProductCategory Category,
    bool IsActive) : IRequest<MenuItemDto>;

public sealed class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, MenuItemDto>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMenuItemCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<MenuItemDto> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _menuRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOrderException("Item do cardápio não encontrado.", isNotFound: true);

        item.Update(request.Name, request.Price, request.Category, request.IsActive);
        _menuRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new MenuItemDto(item.Id, item.Code, item.Name, item.Price, item.Category, item.IsActive);
    }
}

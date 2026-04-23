using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Mediator;

namespace GoodHamburger.Application.Menu.Commands;

public sealed record CreateMenuItemCommand(
    string Code,
    string Name,
    decimal Price,
    ProductCategory Category) : IRequest<MenuItemDto>;

public sealed class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, MenuItemDto>
{
    private readonly IMenuRepository _menuRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMenuItemCommandHandler(IMenuRepository menuRepository, IUnitOfWork unitOfWork)
    {
        _menuRepository = menuRepository;
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<MenuItemDto> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var item = MenuItem.Create(request.Code, request.Name, request.Price, request.Category);
        await _menuRepository.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new MenuItemDto(item.Id, item.Code, item.Name, item.Price, item.Category, item.IsActive);
    }
}

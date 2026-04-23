using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Mediator;

namespace GoodHamburger.Application.Menu.Queries;

public sealed record GetMenuItemByIdQuery(Guid Id) : IRequest<MenuItemDto?>;

public sealed class GetMenuItemByIdQueryHandler : IRequestHandler<GetMenuItemByIdQuery, MenuItemDto?>
{
    private readonly IMenuRepository _menuRepository;

    public GetMenuItemByIdQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async ValueTask<MenuItemDto?> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _menuRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null) return null;
        return new MenuItemDto(item.Id, item.Code, item.Name, item.Price, item.Category, item.IsActive);
    }
}

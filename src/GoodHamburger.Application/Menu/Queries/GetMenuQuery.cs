using GoodHamburger.Application.Abstractions;
using GoodHamburger.Application.Dtos;
using Mediator;

namespace GoodHamburger.Application.Menu.Queries;

public sealed record GetMenuQuery : IRequest<IReadOnlyList<MenuItemDto>>;

public sealed class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, IReadOnlyList<MenuItemDto>>
{
    private readonly IMenuRepository _menuRepository;

    public GetMenuQueryHandler(IMenuRepository menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async ValueTask<IReadOnlyList<MenuItemDto>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
    {
        var items = await _menuRepository.GetAllAsync(cancellationToken);
        return items.Select(i => new MenuItemDto(i.Id, i.Code, i.Name, i.Price, i.Category)).ToList();
    }
}

using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly AppDbContext _context;

    public MenuRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.MenuItems.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MenuItem>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.MenuItems
            .Where(m => idList.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }
}

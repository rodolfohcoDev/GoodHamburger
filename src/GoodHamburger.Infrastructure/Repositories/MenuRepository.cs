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

    public async Task<IReadOnlyList<MenuItem>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.MenuItems.AsQueryable();
        if (!includeInactive)
            query = query.Where(m => m.IsActive);
        return await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MenuItem>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.MenuItems
            .Where(m => idList.Contains(m.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.MenuItems.FindAsync([id], cancellationToken);

    public async Task AddAsync(MenuItem item, CancellationToken cancellationToken = default)
        => await _context.MenuItems.AddAsync(item, cancellationToken);

    public void Update(MenuItem item)
        => _context.MenuItems.Update(item);

    public void Delete(MenuItem item)
        => _context.MenuItems.Remove(item);

    public async Task<bool> AllExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        var count = await _context.MenuItems.CountAsync(m => idList.Contains(m.Id), cancellationToken);
        return count == idList.Count;
    }
}


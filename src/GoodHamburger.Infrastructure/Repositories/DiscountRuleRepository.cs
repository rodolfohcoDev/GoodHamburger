using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Repositories;

public class DiscountRuleRepository : IDiscountRepository
{
    private readonly AppDbContext _context;

    public DiscountRuleRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<DiscountRule>> GetAllAsync(CancellationToken ct = default)
        => await _context.DiscountRules
            .Include(r => r.RequiredItems)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<DiscountRule>> ListActiveOrderedByPriorityAsync(CancellationToken ct = default)
        => await _context.DiscountRules
            .Where(r => r.IsActive)
            .Include(r => r.RequiredItems)
            .OrderBy(r => r.Priority)
            .ToListAsync(ct);

    public async Task<DiscountRule?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.DiscountRules
            .Include(r => r.RequiredItems)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(DiscountRule rule, CancellationToken ct = default)
        => await _context.DiscountRules.AddAsync(rule, ct);

    public void Update(DiscountRule rule)
        => _context.DiscountRules.Update(rule);

    public void Delete(DiscountRule rule)
        => _context.DiscountRules.Remove(rule);
}

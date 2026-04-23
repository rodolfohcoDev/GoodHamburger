using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Abstractions;

public interface IDiscountRepository
{
    Task<IReadOnlyList<DiscountRule>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DiscountRule>> ListActiveOrderedByPriorityAsync(CancellationToken ct = default);
    Task<DiscountRule?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(DiscountRule rule, CancellationToken ct = default);
    void Update(DiscountRule rule);
    void Delete(DiscountRule rule);
}

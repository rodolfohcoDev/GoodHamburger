using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Abstractions;

public interface IMenuRepository
{
    Task<IReadOnlyList<MenuItem>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(MenuItem item, CancellationToken cancellationToken = default);
    void Update(MenuItem item);
    void Delete(MenuItem item);
    Task<bool> AllExistAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}

using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Abstractions;

public interface IMenuRepository
{
    Task<IReadOnlyList<MenuItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MenuItem>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}

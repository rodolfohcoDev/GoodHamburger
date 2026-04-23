namespace GoodHamburger.Application.Dtos;

public record SimulateDiscountRequest(
    IReadOnlyCollection<Guid> MenuItemIds,
    DateTime? AtUtc = null);

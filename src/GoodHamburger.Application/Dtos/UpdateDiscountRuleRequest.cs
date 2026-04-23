using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Dtos;

public record UpdateDiscountRuleRequest(
    string Name,
    decimal Percent,
    DiscountMatchMode MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    IReadOnlyCollection<Guid>? RequiredMenuItemIds,
    int Priority,
    bool IsActive,
    decimal? MinimumSubtotal = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null);

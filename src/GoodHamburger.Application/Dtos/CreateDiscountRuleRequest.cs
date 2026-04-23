using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Dtos;

public record CreateDiscountRuleRequest(
    string Name,
    decimal Percent,
    DiscountMatchMode MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    IReadOnlyCollection<Guid>? RequiredMenuItemIds,
    int Priority,
    bool IsActive = true,
    decimal? MinimumSubtotal = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null);

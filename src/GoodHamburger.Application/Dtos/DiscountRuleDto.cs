using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Dtos;

public record DiscountRuleDto(
    Guid Id,
    string Name,
    decimal Percent,
    DiscountMatchMode MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    IReadOnlyCollection<Guid> RequiredMenuItemIds,
    int Priority,
    bool IsActive,
    decimal? MinimumSubtotal,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string Fingerprint);

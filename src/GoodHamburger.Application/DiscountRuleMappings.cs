using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application;

internal static class DiscountRuleMappings
{
    internal static DiscountRuleDto ToDto(this DiscountRule rule) => new(
        rule.Id,
        rule.Name,
        rule.Percent,
        rule.MatchMode,
        rule.RequiresSandwich,
        rule.RequiresFries,
        rule.RequiresDrink,
        rule.RequiredMenuItemIds,
        rule.Priority,
        rule.IsActive,
        rule.MinimumSubtotal,
        rule.ValidFrom,
        rule.ValidUntil,
        rule.Fingerprint);
}

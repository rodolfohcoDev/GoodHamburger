using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Persistence.Seed;

public static class DiscountRuleSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.DiscountRules.AnyAsync(cancellationToken))
            return;

        var rules = new[]
        {
            DiscountRule.Create(
                name: "Combo Completo",
                percent: 20m,
                matchMode: DiscountMatchMode.CategoryAtLeast,
                requiresSandwich: true, requiresFries: true, requiresDrink: true,
                requiredMenuItemIds: null,
                priority: 1, isActive: true,
                minimumSubtotal: null, validFrom: null, validUntil: null),

            DiscountRule.Create(
                name: "Sanduíche + Bebida",
                percent: 15m,
                matchMode: DiscountMatchMode.CategoryAtLeast,
                requiresSandwich: true, requiresFries: false, requiresDrink: true,
                requiredMenuItemIds: null,
                priority: 2, isActive: true,
                minimumSubtotal: null, validFrom: null, validUntil: null),

            DiscountRule.Create(
                name: "Sanduíche + Batata",
                percent: 10m,
                matchMode: DiscountMatchMode.CategoryAtLeast,
                requiresSandwich: true, requiresFries: true, requiresDrink: false,
                requiredMenuItemIds: null,
                priority: 3, isActive: true,
                minimumSubtotal: null, validFrom: null, validUntil: null),
        };

        context.DiscountRules.AddRange(rules);
        await context.SaveChangesAsync(cancellationToken);
    }
}

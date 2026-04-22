using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Services;

public class DiscountPolicy : IDiscountPolicy
{
    private static readonly IReadOnlyList<(Func<IReadOnlyCollection<OrderItem>, bool> Condition, decimal Percent)> Rules =
    [
        (items => HasCategory(items, ProductCategory.Sandwich)
                  && HasCategory(items, ProductCategory.Fries)
                  && HasCategory(items, ProductCategory.Drink), 20m),

        (items => HasCategory(items, ProductCategory.Sandwich)
                  && HasCategory(items, ProductCategory.Drink), 15m),

        (items => HasCategory(items, ProductCategory.Sandwich)
                  && HasCategory(items, ProductCategory.Fries), 10m),
    ];

    public decimal CalculatePercent(IReadOnlyCollection<OrderItem> items)
    {
        foreach (var (condition, percent) in Rules)
        {
            if (condition(items))
                return percent;
        }
        return 0m;
    }

    private static bool HasCategory(IReadOnlyCollection<OrderItem> items, ProductCategory category)
        => items.Any(i => i.Category == category);
}

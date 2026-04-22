using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Persistence.Seed;

public static class MenuSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.MenuItems.AnyAsync(cancellationToken))
            return;

        var items = new[]
        {
            MenuItem.Create("XBURGER", "X Burger",  5.00m, ProductCategory.Sandwich),
            MenuItem.Create("XEGG",    "X Egg",     4.50m, ProductCategory.Sandwich),
            MenuItem.Create("XBACON",  "X Bacon",   7.00m, ProductCategory.Sandwich),
            MenuItem.Create("FRIES",   "Batata frita", 2.00m, ProductCategory.Fries),
            MenuItem.Create("SODA",    "Refrigerante", 2.50m, ProductCategory.Drink),
        };

        context.MenuItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);
    }
}

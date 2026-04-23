using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Infrastructure.Persistence;
using GoodHamburger.Infrastructure.Persistence.Seed;
using GoodHamburger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GoodHamburger.Infrastructure.Tests;

public static class DbContextFactory
{
    public static AppDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }
}

public class OrderRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsOrder()
    {
        using var ctx = DbContextFactory.Create(nameof(AddAsync_PersistsOrder));
        var repo = new OrderRepository(ctx);

        var order = Order.Create();
        await repo.AddAsync(order);
        await ctx.SaveChangesAsync();

        var saved = await ctx.Orders.FindAsync(order.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrderWithItems()
    {
        using var ctx = DbContextFactory.Create(nameof(GetByIdAsync_ReturnsOrderWithItems));
        var sandwich = MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);
        ctx.MenuItems.Add(sandwich);
        var order = Order.Create();
        order.AddItem(sandwich);
        ctx.Orders.Add(order);
        await ctx.SaveChangesAsync();

        var repo = new OrderRepository(ctx);
        var result = await repo.GetByIdAsync(order.Id);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListAsync_PaginatesCorrectly()
    {
        using var ctx = DbContextFactory.Create(nameof(ListAsync_PaginatesCorrectly));
        for (int i = 0; i < 5; i++)
        {
            ctx.Orders.Add(Order.Create());
        }
        await ctx.SaveChangesAsync();

        var repo = new OrderRepository(ctx);
        var (items, total) = await repo.ListAsync(1, 3);

        items.Should().HaveCount(3);
        total.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var ctx = DbContextFactory.Create(nameof(GetByIdAsync_NotFound_ReturnsNull));
        var repo = new OrderRepository(ctx);

        var result = await repo.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }
}

public class MenuRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSeedItems()
    {
        using var ctx = DbContextFactory.Create(nameof(GetAllAsync_ReturnsSeedItems));
        await MenuSeeder.SeedAsync(ctx);

        var repo = new MenuRepository(ctx);
        var items = await repo.GetAllAsync();

        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsMatchingItems()
    {
        using var ctx = DbContextFactory.Create(nameof(GetByIdsAsync_ReturnsMatchingItems));
        var item = MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);
        ctx.MenuItems.Add(item);
        await ctx.SaveChangesAsync();

        var repo = new MenuRepository(ctx);
        var result = await repo.GetByIdsAsync([item.Id]);

        result.Should().HaveCount(1);
        result[0].Code.Should().Be("XBURGER");
    }

    [Fact]
    public async Task AllExistAsync_AllPresent_ReturnsTrue()
    {
        using var ctx = DbContextFactory.Create(nameof(AllExistAsync_AllPresent_ReturnsTrue));
        var item1 = MenuItem.Create("XBURGER", "X Burger", 5m, ProductCategory.Sandwich);
        var item2 = MenuItem.Create("FRIES", "Batata", 2m, ProductCategory.Fries);
        ctx.MenuItems.AddRange(item1, item2);
        await ctx.SaveChangesAsync();

        var repo = new MenuRepository(ctx);
        var result = await repo.AllExistAsync([item1.Id, item2.Id]);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AllExistAsync_SomeMissing_ReturnsFalse()
    {
        using var ctx = DbContextFactory.Create(nameof(AllExistAsync_SomeMissing_ReturnsFalse));
        var item = MenuItem.Create("XBURGER", "X Burger", 5m, ProductCategory.Sandwich);
        ctx.MenuItems.Add(item);
        await ctx.SaveChangesAsync();

        var repo = new MenuRepository(ctx);
        var result = await repo.AllExistAsync([item.Id, Guid.NewGuid()]);
        result.Should().BeFalse();
    }
}

public class DiscountRuleRepositoryTests
{
    [Fact]
    public async Task AddAsync_PersistsRule()
    {
        using var ctx = DbContextFactory.Create(nameof(AddAsync_PersistsRule));
        var repo = new DiscountRuleRepository(ctx);

        var rule = DiscountRule.Create("Combo", 20m, DiscountMatchMode.CategoryAtLeast,
            true, true, true, null, 1, true, null, null, null);
        await repo.AddAsync(rule);
        await ctx.SaveChangesAsync();

        var saved = await ctx.DiscountRules.Include(r => r.RequiredItems)
            .FirstOrDefaultAsync(r => r.Id == rule.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Combo");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllRules()
    {
        using var ctx = DbContextFactory.Create(nameof(GetAllAsync_ReturnsAllRules));
        await DiscountRuleSeeder.SeedAsync(ctx);

        var repo = new DiscountRuleRepository(ctx);
        var rules = await repo.GetAllAsync();

        rules.Should().HaveCount(3);
    }

    [Fact]
    public async Task ListActiveOrderedByPriorityAsync_ReturnsOnlyActive()
    {
        using var ctx = DbContextFactory.Create(nameof(ListActiveOrderedByPriorityAsync_ReturnsOnlyActive));
        var active = DiscountRule.Create("Active", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, null, null);
        var inactive = DiscountRule.Create("Inactive", 10m, DiscountMatchMode.CategoryAtLeast,
            true, true, false, null, 2, false, null, null, null);
        ctx.DiscountRules.AddRange(active, inactive);
        await ctx.SaveChangesAsync();

        var repo = new DiscountRuleRepository(ctx);
        var rules = await repo.ListActiveOrderedByPriorityAsync();
        rules.Should().HaveCount(1);
        rules[0].Name.Should().Be("Active");
    }

    [Fact]
    public async Task GetByIdAsync_IncludesRequiredItems()
    {
        using var ctx = DbContextFactory.Create(nameof(GetByIdAsync_IncludesRequiredItems));
        await MenuSeeder.SeedAsync(ctx);
        var menuItem = ctx.MenuItems.First();

        var rule = DiscountRule.Create("Specific", 5m, DiscountMatchMode.SpecificItems,
            false, false, false, [menuItem.Id], 1, true, null, null, null);
        ctx.DiscountRules.Add(rule);
        await ctx.SaveChangesAsync();

        var repo = new DiscountRuleRepository(ctx);
        var loaded = await repo.GetByIdAsync(rule.Id);

        loaded.Should().NotBeNull();
        loaded!.RequiredMenuItemIds.Should().Contain(menuItem.Id);
    }

    [Fact]
    public async Task Delete_RemovesRule()
    {
        using var ctx = DbContextFactory.Create(nameof(Delete_RemovesRule));
        var rule = DiscountRule.Create("ToDelete", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, null, null);
        ctx.DiscountRules.Add(rule);
        await ctx.SaveChangesAsync();

        var repo = new DiscountRuleRepository(ctx);
        var loaded = await repo.GetByIdAsync(rule.Id);
        repo.Delete(loaded!);
        await ctx.SaveChangesAsync();

        var deleted = await ctx.DiscountRules.FindAsync(rule.Id);
        deleted.Should().BeNull();
    }
}

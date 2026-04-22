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
}

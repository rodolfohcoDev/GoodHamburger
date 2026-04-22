using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Services;
using Xunit;

namespace GoodHamburger.Domain.Tests;

public class DiscountPolicyTests
{
    private readonly DiscountPolicy _policy = new();

    private static MenuItem MakeSandwich(string code = "XBURGER") =>
        MenuItem.Create(code, "X Burger", 5.00m, ProductCategory.Sandwich);

    private static MenuItem MakeFries() =>
        MenuItem.Create("FRIES", "Batata frita", 2.00m, ProductCategory.Fries);

    private static MenuItem MakeDrink() =>
        MenuItem.Create("SODA", "Refrigerante", 2.50m, ProductCategory.Drink);

    private static IReadOnlyCollection<OrderItem> BuildItems(params MenuItem[] menuItems)
    {
        var order = Order.Create();
        foreach (var item in menuItems)
            order.AddItem(item);
        return order.Items;
    }

    [Fact]
    public void EmptyOrder_Returns0Percent()
    {
        var items = BuildItems();
        _policy.CalculatePercent(items).Should().Be(0m);
    }

    [Fact]
    public void OnlySandwich_Returns0Percent()
    {
        var items = BuildItems(MakeSandwich());
        _policy.CalculatePercent(items).Should().Be(0m);
    }

    [Fact]
    public void OnlyFries_Returns0Percent()
    {
        var items = BuildItems(MakeFries());
        _policy.CalculatePercent(items).Should().Be(0m);
    }

    [Fact]
    public void OnlyDrink_Returns0Percent()
    {
        var items = BuildItems(MakeDrink());
        _policy.CalculatePercent(items).Should().Be(0m);
    }

    [Fact]
    public void SandwichAndFries_Returns10Percent()
    {
        var items = BuildItems(MakeSandwich(), MakeFries());
        _policy.CalculatePercent(items).Should().Be(10m);
    }

    [Fact]
    public void SandwichAndDrink_Returns15Percent()
    {
        var items = BuildItems(MakeSandwich(), MakeDrink());
        _policy.CalculatePercent(items).Should().Be(15m);
    }

    [Fact]
    public void SandwichAndFriesAndDrink_Returns20Percent()
    {
        var items = BuildItems(MakeSandwich(), MakeFries(), MakeDrink());
        _policy.CalculatePercent(items).Should().Be(20m);
    }

    [Fact]
    public void FriesAndDrinkWithoutSandwich_Returns0Percent()
    {
        var items = BuildItems(MakeFries(), MakeDrink());
        _policy.CalculatePercent(items).Should().Be(0m);
    }

    [Theory]
    [InlineData("XBURGER", "X Burger", 5.00)]
    [InlineData("XEGG", "X Egg", 4.50)]
    [InlineData("XBACON", "X Bacon", 7.00)]
    public void AllSandwiches_WithCombo_Returns20Percent(string code, string name, double price)
    {
        var sandwich = MenuItem.Create(code, name, (decimal)price, ProductCategory.Sandwich);
        var items = BuildItems(sandwich, MakeFries(), MakeDrink());
        _policy.CalculatePercent(items).Should().Be(20m);
    }

    [Fact]
    public void ComboRuleWins_Over_SandwichDrinkRule()
    {
        var items = BuildItems(MakeSandwich(), MakeFries(), MakeDrink());
        _policy.CalculatePercent(items).Should().Be(20m);
    }
}

public class OrderTests
{
    private static MenuItem MakeSandwich() =>
        MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);

    private static MenuItem MakeFries() =>
        MenuItem.Create("FRIES", "Batata frita", 2.00m, ProductCategory.Fries);

    private static MenuItem MakeDrink() =>
        MenuItem.Create("SODA", "Refrigerante", 2.50m, ProductCategory.Drink);

    [Fact]
    public void Create_ReturnsEmptyOrder()
    {
        var order = Order.Create();
        order.Should().NotBeNull();
        order.Items.Should().BeEmpty();
        order.Total.Should().Be(0m);
        order.Subtotal.Should().Be(0m);
    }

    [Fact]
    public void AddItem_AddsCorrectly()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich());
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void AddItem_DuplicateCategory_ThrowsDuplicateItemException()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich());

        var act = () => order.AddItem(MenuItem.Create("XBACON", "X Bacon", 7.00m, ProductCategory.Sandwich));
        act.Should().Throw<DuplicateItemException>()
            .WithMessage("*Sandwich*");
    }

    [Fact]
    public void RemoveItem_RemovesCorrectly()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich());
        var itemId = order.Items.First().Id;

        order.RemoveItem(itemId);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void ReplaceItems_ReplacesAll()
    {
        var order = Order.Create();
        order.AddItem(MakeSandwich());
        order.ReplaceItems([MakeFries(), MakeDrink()]);
        order.Items.Should().HaveCount(2);
        order.Items.Should().NotContain(i => i.Category == ProductCategory.Sandwich);
    }

    [Fact]
    public void Recalculate_ComboComplete_CorrectValues()
    {
        // XBacon(7.00) + Fries(2.00) + Soda(2.50) = 11.50, 20% discount = 2.30, total = 9.20
        var order = Order.Create();
        order.AddItem(MenuItem.Create("XBACON", "X Bacon", 7.00m, ProductCategory.Sandwich));
        order.AddItem(MakeFries());
        order.AddItem(MakeDrink());
        var policy = new DiscountPolicy();

        order.Recalculate(policy);

        order.Subtotal.Should().Be(11.50m);
        order.DiscountPercent.Should().Be(20m);
        order.DiscountAmount.Should().Be(2.30m);
        order.Total.Should().Be(9.20m);
    }

    [Fact]
    public void Recalculate_RoundingCorrect()
    {
        // XEgg(4.50) + Soda(2.50) = 7.00, 15% = 1.05, total = 5.95
        var order = Order.Create();
        order.AddItem(MenuItem.Create("XEGG", "X Egg", 4.50m, ProductCategory.Sandwich));
        order.AddItem(MakeDrink());
        var policy = new DiscountPolicy();

        order.Recalculate(policy);

        order.Subtotal.Should().Be(7.00m);
        order.DiscountPercent.Should().Be(15m);
        order.DiscountAmount.Should().Be(1.05m);
        order.Total.Should().Be(5.95m);
    }
}

public class MenuItemTests
{
    [Fact]
    public void Create_WithEmptyName_ThrowsArgumentException()
    {
        var act = () => MenuItem.Create("CODE", "", 5.00m, ProductCategory.Sandwich);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ThrowsArgumentException()
    {
        var act = () => MenuItem.Create("CODE", "Item", -1m, ProductCategory.Sandwich);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ValidData_CreatesMenuItem()
    {
        var item = MenuItem.Create("XBURGER", "X Burger", 5.00m, ProductCategory.Sandwich);
        item.Should().NotBeNull();
        item.Code.Should().Be("XBURGER");
        item.Price.Should().Be(5.00m);
    }
}

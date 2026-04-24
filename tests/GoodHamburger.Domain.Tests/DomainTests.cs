using FluentAssertions;
using GoodHamburger.Domain.Entities;
using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;
using Xunit;

namespace GoodHamburger.Domain.Tests;

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
    public void ApplyDiscount_ComboComplete_CorrectValues()
    {
        // XBacon(7.00) + Fries(2.00) + Soda(2.50) = 11.50, 20% discount = 2.30, total = 9.20
        var order = Order.Create();
        order.AddItem(MenuItem.Create("XBACON", "X Bacon", 7.00m, ProductCategory.Sandwich));
        order.AddItem(MakeFries());
        order.AddItem(MakeDrink());
        var ruleId = Guid.NewGuid();

        order.ApplyDiscount(20m, ruleId, "Combo Completo");

        order.Subtotal.Should().Be(11.50m);
        order.DiscountPercent.Should().Be(20m);
        order.DiscountAmount.Should().Be(2.30m);
        order.Total.Should().Be(9.20m);
        order.AppliedDiscountRuleId.Should().Be(ruleId);
        order.AppliedDiscountRuleName.Should().Be("Combo Completo");
    }

    [Fact]
    public void ApplyDiscount_RoundingCorrect()
    {
        // XEgg(4.50) + Soda(2.50) = 7.00, 15% = 1.05, total = 5.95
        var order = Order.Create();
        order.AddItem(MenuItem.Create("XEGG", "X Egg", 4.50m, ProductCategory.Sandwich));
        order.AddItem(MakeDrink());

        order.ApplyDiscount(15m, null, null);

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

public class DiscountRuleMatchModeTests
{
    private static MenuItem MakeSandwich() => MenuItem.Create("XBURGER", "X Burger", 5m, ProductCategory.Sandwich);
    private static MenuItem MakeFries()    => MenuItem.Create("FRIES", "Batata", 2m, ProductCategory.Fries);
    private static MenuItem MakeDrink()    => MenuItem.Create("SODA", "Soda", 2.5m, ProductCategory.Drink);

    private static IReadOnlyCollection<OrderItem> Items(params MenuItem[] items)
        => Order.BuildSimulationItems(items);

    // ── CategoryAtLeast tests ──────────────────────────────────────────

    [Fact]
    public void CategoryAtLeast_Combo_MatchesCombo()
    {
        var rule = DiscountRule.Create("Combo", 20m, DiscountMatchMode.CategoryAtLeast,
            true, true, true, null, 1, true, null, null, null);
        rule.Matches(Items(MakeSandwich(), MakeFries(), MakeDrink()), 9.5m, DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void CategoryAtLeast_Combo_DoesNotMatchSandwichOnly()
    {
        var rule = DiscountRule.Create("Combo", 20m, DiscountMatchMode.CategoryAtLeast,
            true, true, true, null, 1, true, null, null, null);
        rule.Matches(Items(MakeSandwich()), 5m, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void CategoryAtLeast_SandwichFries_MatchesTwoItems()
    {
        var rule = DiscountRule.Create("SF", 10m, DiscountMatchMode.CategoryAtLeast,
            true, true, false, null, 1, true, null, null, null);
        rule.Matches(Items(MakeSandwich(), MakeFries()), 7m, DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void CategoryAtLeast_SandwichFries_AlsoMatchesCombo()
    {
        // CategoryAtLeast: having sandwich+fries+drink ALSO matches a rule requiring sandwich+fries
        var rule = DiscountRule.Create("SF", 10m, DiscountMatchMode.CategoryAtLeast,
            true, true, false, null, 1, true, null, null, null);
        rule.Matches(Items(MakeSandwich(), MakeFries(), MakeDrink()), 9.5m, DateTime.UtcNow).Should().BeTrue();
    }

    // ── CategoryExact tests ────────────────────────────────────────────

    [Fact]
    public void CategoryExact_Combo_DoesNotMatchComboWithExtra()
    {
        var rule = DiscountRule.Create("Exact2", 15m, DiscountMatchMode.CategoryExact,
            true, false, true, null, 1, true, null, null, null);
        // Rule requires exactly sandwich+drink (2 items). Sending 3 items should NOT match.
        rule.Matches(Items(MakeSandwich(), MakeFries(), MakeDrink()), 9.5m, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void CategoryExact_SandwichDrink_MatchesExactly2()
    {
        var rule = DiscountRule.Create("Exact2", 15m, DiscountMatchMode.CategoryExact,
            true, false, true, null, 1, true, null, null, null);
        rule.Matches(Items(MakeSandwich(), MakeDrink()), 7.5m, DateTime.UtcNow).Should().BeTrue();
    }

    // ── Validity window tests ──────────────────────────────────────────

    [Fact]
    public void Matches_InactiveRule_ReturnsFalse()
    {
        var rule = DiscountRule.Create("Inactive", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, false, null, null, null);
        rule.Matches(Items(MakeSandwich()), 5m, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Matches_BeforeValidFrom_ReturnsFalse()
    {
        var future = DateTime.UtcNow.AddDays(1);
        var rule = DiscountRule.Create("Future", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, future, null);
        rule.Matches(Items(MakeSandwich()), 5m, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Matches_AfterValidUntil_ReturnsFalse()
    {
        var past = DateTime.UtcNow.AddDays(-1);
        var rule = DiscountRule.Create("Expired", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, null, past);
        rule.Matches(Items(MakeSandwich()), 5m, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Matches_BelowMinimumSubtotal_ReturnsFalse()
    {
        var rule = DiscountRule.Create("MinSub", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, 10m, null, null);
        rule.Matches(Items(MakeSandwich()), 5m, DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Matches_AtMinimumSubtotal_ReturnsTrue()
    {
        var rule = DiscountRule.Create("MinSub", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, 5m, null, null);
        rule.Matches(Items(MakeSandwich()), 5m, DateTime.UtcNow).Should().BeTrue();
    }

    // ── Fingerprint tests ──────────────────────────────────────────────

    [Fact]
    public void Fingerprint_SameParams_AreEqual()
    {
        var rule1 = DiscountRule.Create("A", 10m, DiscountMatchMode.CategoryAtLeast,
            true, true, false, null, 1, true, null, null, null);
        var rule2 = DiscountRule.Create("B", 20m, DiscountMatchMode.CategoryAtLeast,
            true, true, false, null, 2, false, null, null, null);
        rule1.Fingerprint.Should().Be(rule2.Fingerprint);
    }

    [Fact]
    public void Fingerprint_DifferentParams_AreDifferent()
    {
        var rule1 = DiscountRule.Create("A", 10m, DiscountMatchMode.CategoryAtLeast,
            true, true, false, null, 1, true, null, null, null);
        var rule2 = DiscountRule.Create("B", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, true, null, 1, true, null, null, null);
        rule1.Fingerprint.Should().NotBe(rule2.Fingerprint);
    }
}

public class DiscountRuleInvariantTests
{
    [Fact]
    public void Create_EmptyName_Throws()
    {
        var act = () => DiscountRule.Create("", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, null, null);
        act.Should().Throw<InvalidDiscountRuleException>();
    }

    [Fact]
    public void Create_ZeroPercent_Throws()
    {
        var act = () => DiscountRule.Create("Rule", 0m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null, null, null);
        act.Should().Throw<InvalidDiscountRuleException>();
    }

    [Fact]
    public void Create_NoCategories_Throws()
    {
        var act = () => DiscountRule.Create("Rule", 10m, DiscountMatchMode.CategoryAtLeast,
            false, false, false, null, 1, true, null, null, null);
        act.Should().Throw<InvalidDiscountRuleException>();
    }

    [Fact]
    public void Create_ValidUntilBeforeValidFrom_Throws()
    {
        var act = () => DiscountRule.Create("Rule", 10m, DiscountMatchMode.CategoryAtLeast,
            true, false, false, null, 1, true, null,
            DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(1));
        act.Should().Throw<InvalidDiscountRuleException>();
    }

    [Fact]
    public void Create_SpecificItemsNoIds_Throws()
    {
        var act = () => DiscountRule.Create("Rule", 10m, DiscountMatchMode.SpecificItems,
            false, false, false, [], 1, true, null, null, null);
        act.Should().Throw<InvalidDiscountRuleException>();
    }

    [Fact]
    public void Order_ApplyDiscount_SetsFields()
    {
        var order = Order.Create();
        order.AddItem(MenuItem.Create("XBURGER", "X Burger", 5m, ProductCategory.Sandwich));
        var ruleId = Guid.NewGuid();

        order.ApplyDiscount(10m, ruleId, "Test Rule");

        order.DiscountPercent.Should().Be(10m);
        order.AppliedDiscountRuleId.Should().Be(ruleId);
        order.AppliedDiscountRuleName.Should().Be("Test Rule");
        order.DiscountAmount.Should().Be(0.50m);
        order.Total.Should().Be(4.50m);
    }
}

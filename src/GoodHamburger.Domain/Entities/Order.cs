using GoodHamburger.Domain.Exceptions;
using GoodHamburger.Domain.Services;

namespace GoodHamburger.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal Total { get; private set; }
    public Guid? AppliedDiscountRuleId { get; private set; }
    public string? AppliedDiscountRuleName { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create()
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(MenuItem item)
    {
        if (_items.Any(i => i.Category == item.Category))
            throw new DuplicateItemException(item.Category.ToString());

        _items.Add(OrderItem.Create(Id, item));
    }

    public void RemoveItem(Guid orderItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == orderItemId);
        if (item is not null)
            _items.Remove(item);
    }

    public void ReplaceItems(IEnumerable<MenuItem> items)
    {
        _items.Clear();
        foreach (var item in items)
            AddItem(item);
    }

    public void Recalculate(IDiscountPolicy policy)
    {
        Subtotal = _items.Sum(i => i.UnitPrice);
        DiscountPercent = policy.CalculatePercent(_items);
        DiscountAmount = Math.Round(Subtotal * (DiscountPercent / 100), 2, MidpointRounding.AwayFromZero);
        Total = Subtotal - DiscountAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies a pre-calculated discount (from the parameterized discount policy).
    /// Recalculates Subtotal from items, then applies the given percent.
    /// </summary>
    public void ApplyDiscount(decimal percent, Guid? ruleId, string? ruleName)
    {
        Subtotal = _items.Sum(i => i.UnitPrice);
        DiscountPercent = percent;
        DiscountAmount = Math.Round(Subtotal * (DiscountPercent / 100), 2, MidpointRounding.AwayFromZero);
        Total = Subtotal - DiscountAmount;
        AppliedDiscountRuleId = ruleId;
        AppliedDiscountRuleName = ruleName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Builds a provisional in-memory list of OrderItems for discount simulation
    /// (no category-duplicate check, no persistence).
    /// </summary>
    public static IReadOnlyCollection<OrderItem> BuildSimulationItems(IEnumerable<MenuItem> menuItems)
    {
        var tempOrder = new Order { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        foreach (var item in menuItems)
            tempOrder._items.Add(OrderItem.Create(tempOrder.Id, item));
        return tempOrder.Items;
    }
}

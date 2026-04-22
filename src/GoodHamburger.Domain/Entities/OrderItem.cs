using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public string MenuItemName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public ProductCategory Category { get; private set; }

    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, MenuItem menuItem)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            MenuItemId = menuItem.Id,
            MenuItemName = menuItem.Name,
            UnitPrice = menuItem.Price,
            Category = menuItem.Category
        };
    }
}

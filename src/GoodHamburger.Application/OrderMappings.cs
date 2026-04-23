using GoodHamburger.Application.Dtos;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application;

internal static class OrderMappings
{
    internal static OrderResponse ToResponse(this Order order)
    {
        var items = order.Items
            .Select(i => new OrderItemDto(i.Id, i.MenuItemId, i.MenuItemName, i.UnitPrice, i.Category))
            .ToList();

        return new OrderResponse(
            order.Id,
            order.CreatedAt,
            order.UpdatedAt,
            items,
            order.Subtotal,
            order.DiscountPercent,
            order.DiscountAmount,
            order.Total,
            order.AppliedDiscountRuleId,
            order.AppliedDiscountRuleName);
    }
}

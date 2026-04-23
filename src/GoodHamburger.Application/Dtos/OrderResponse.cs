namespace GoodHamburger.Application.Dtos;

public record OrderResponse(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OrderItemDto> Items,
    decimal Subtotal,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal Total,
    Guid? AppliedDiscountRuleId = null,
    string? AppliedDiscountRuleName = null);

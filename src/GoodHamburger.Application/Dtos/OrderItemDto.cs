using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Dtos;

public record OrderItemDto(Guid Id, Guid MenuItemId, string MenuItemName, decimal UnitPrice, ProductCategory Category);

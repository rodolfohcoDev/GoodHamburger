using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Application.Dtos;

public record MenuItemDto(Guid Id, string Code, string Name, decimal Price, ProductCategory Category, bool IsActive);

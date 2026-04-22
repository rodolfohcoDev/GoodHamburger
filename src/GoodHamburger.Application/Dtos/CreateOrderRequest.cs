namespace GoodHamburger.Application.Dtos;

public record CreateOrderRequest(List<Guid> MenuItemIds);

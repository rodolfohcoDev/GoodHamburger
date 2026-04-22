namespace GoodHamburger.Application.Dtos;

public record UpdateOrderRequest(List<Guid> MenuItemIds);

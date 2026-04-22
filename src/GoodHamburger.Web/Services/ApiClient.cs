using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Web.Services;

public record MenuItemModel(Guid Id, string Code, string Name, decimal Price, ProductCategory Category);
public record OrderItemModel(Guid Id, Guid MenuItemId, string MenuItemName, decimal UnitPrice, ProductCategory Category);
public record OrderModel(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OrderItemModel> Items,
    decimal Subtotal,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal Total);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MenuItemModel>> GetMenuAsync()
        => await _http.GetFromJsonAsync<List<MenuItemModel>>("/api/menu") ?? [];

    public async Task<PagedResult<OrderModel>> GetOrdersAsync(int page = 1, int pageSize = 20)
        => await _http.GetFromJsonAsync<PagedResult<OrderModel>>(
            $"/api/orders?page={page}&pageSize={pageSize}") ?? new PagedResult<OrderModel>([], 0, page, pageSize);

    public async Task<OrderModel?> GetOrderAsync(Guid id)
        => await _http.GetFromJsonAsync<OrderModel>($"/api/orders/{id}");

    public async Task<(OrderModel? Order, string? Error)> CreateOrderAsync(List<Guid> menuItemIds)
    {
        var response = await _http.PostAsJsonAsync("/api/orders", new { MenuItemIds = menuItemIds });
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<OrderModel>(), null);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return (null, problem?.Detail ?? "Erro desconhecido.");
    }

    public async Task<string?> DeleteOrderAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/orders/{id}");
        if (response.IsSuccessStatusCode) return null;
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return problem?.Detail ?? "Erro ao excluir pedido.";
    }

    private record ProblemDetails(string? Title, string? Detail);
}

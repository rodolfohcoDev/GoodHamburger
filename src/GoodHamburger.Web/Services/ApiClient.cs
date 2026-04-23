using GoodHamburger.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Web.Services;

public record MenuItemModel(Guid Id, string Code, string Name, decimal Price, ProductCategory Category, bool IsActive);
public record OrderItemModel(Guid Id, Guid MenuItemId, string MenuItemName, decimal UnitPrice, ProductCategory Category);
public record OrderModel(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OrderItemModel> Items,
    decimal Subtotal,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal Total,
    Guid? AppliedDiscountRuleId = null,
    string? AppliedDiscountRuleName = null);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

// Discount Rule models
public record DiscountRuleModel(
    Guid Id,
    string Name,
    decimal Percent,
    int MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    List<Guid> RequiredMenuItemIds,
    int Priority,
    bool IsActive,
    decimal? MinimumSubtotal,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string Fingerprint);

public record RuleEvaluation(Guid RuleId, string RuleName, int Priority, bool Matched, string Reason);

public record SimulationResult(
    decimal Subtotal,
    decimal AppliedPercent,
    decimal DiscountAmount,
    decimal Total,
    Guid? AppliedRuleId,
    string? AppliedRuleName,
    List<RuleEvaluation> Evaluations);

public record CreateDiscountRuleRequest(
    string Name,
    decimal Percent,
    int MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    List<Guid>? RequiredMenuItemIds,
    int Priority,
    bool IsActive = true,
    decimal? MinimumSubtotal = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null);

public record UpdateDiscountRuleRequest(
    string Name,
    decimal Percent,
    int MatchMode,
    bool RequiresSandwich,
    bool RequiresFries,
    bool RequiresDrink,
    List<Guid>? RequiredMenuItemIds,
    int Priority,
    bool IsActive,
    decimal? MinimumSubtotal = null,
    DateTime? ValidFrom = null,
    DateTime? ValidUntil = null);

// Admin request models
public record CreateMenuItemRequest(
    [property: Required(ErrorMessage = "Código é obrigatório.")]
    [property: MaxLength(50)]
    string Code,
    [property: Required(ErrorMessage = "Nome é obrigatório.")]
    [property: MaxLength(200)]
    string Name,
    [property: Range(0.01, 9999.99, ErrorMessage = "Preço deve ser maior que zero.")]
    decimal Price,
    ProductCategory Category);

public record UpdateMenuItemRequest(
    [property: Required(ErrorMessage = "Nome é obrigatório.")]
    [property: MaxLength(200)]
    string Name,
    [property: Range(0.01, 9999.99, ErrorMessage = "Preço deve ser maior que zero.")]
    decimal Price,
    ProductCategory Category,
    bool IsActive);

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // ── Public menu ──────────────────────────────────────────────────
    public async Task<List<MenuItemModel>> GetMenuAsync(bool includeInactive = false)
    {
        var url = includeInactive ? "/api/admin/menu" : "/api/menu";
        return await _http.GetFromJsonAsync<List<MenuItemModel>>(url) ?? [];
    }

    // ── Orders ───────────────────────────────────────────────────────
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
        return (null, problem?.Detail ?? problem?.Title ?? "Erro desconhecido.");
    }

    public async Task<string?> DeleteOrderAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/orders/{id}");
        if (response.IsSuccessStatusCode) return null;
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return problem?.Detail ?? "Erro ao excluir pedido.";
    }

    // ── Admin: Menu CRUD ─────────────────────────────────────────────
    public async Task<MenuItemModel?> GetMenuItemAsync(Guid id)
    {
        var response = await _http.GetAsync($"/api/admin/menu/{id}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<MenuItemModel>();
        return null;
    }

    public async Task<(MenuItemModel? Item, string? Error)> CreateMenuItemAsync(CreateMenuItemRequest req)
    {
        var response = await _http.PostAsJsonAsync("/api/admin/menu", req);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<MenuItemModel>(), null);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return (null, problem?.Detail ?? problem?.Title ?? "Erro ao criar item.");
    }

    public async Task<(MenuItemModel? Item, string? Error)> UpdateMenuItemAsync(Guid id, UpdateMenuItemRequest req)
    {
        var response = await _http.PutAsJsonAsync($"/api/admin/menu/{id}", req);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<MenuItemModel>(), null);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return (null, problem?.Detail ?? problem?.Title ?? "Erro ao atualizar item.");
    }

    public async Task<string?> DeleteMenuItemAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/admin/menu/{id}");
        if (response.IsSuccessStatusCode) return null;
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return problem?.Detail ?? "Erro ao excluir item.";
    }

    // ── Admin: Discount Rules CRUD ───────────────────────────────────
    public async Task<List<DiscountRuleModel>> GetDiscountRulesAsync()
        => await _http.GetFromJsonAsync<List<DiscountRuleModel>>("/api/discount-rules") ?? [];

    public async Task<DiscountRuleModel?> GetDiscountRuleAsync(Guid id)
    {
        var response = await _http.GetAsync($"/api/discount-rules/{id}");
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<DiscountRuleModel>();
        return null;
    }

    public async Task<(DiscountRuleModel? Rule, string? Error)> CreateDiscountRuleAsync(CreateDiscountRuleRequest req)
    {
        var response = await _http.PostAsJsonAsync("/api/discount-rules", req);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<DiscountRuleModel>(), null);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return (null, problem?.Detail ?? problem?.Title ?? "Erro ao criar regra.");
    }

    public async Task<(DiscountRuleModel? Rule, string? Error)> UpdateDiscountRuleAsync(Guid id, UpdateDiscountRuleRequest req)
    {
        var response = await _http.PutAsJsonAsync($"/api/discount-rules/{id}", req);
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<DiscountRuleModel>(), null);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return (null, problem?.Detail ?? problem?.Title ?? "Erro ao atualizar regra.");
    }

    public async Task<string?> DeleteDiscountRuleAsync(Guid id)
    {
        var response = await _http.DeleteAsync($"/api/discount-rules/{id}");
        if (response.IsSuccessStatusCode) return null;
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return problem?.Detail ?? "Erro ao excluir regra.";
    }

    public async Task<(SimulationResult? Result, string? Error)> SimulateDiscountAsync(List<Guid> menuItemIds, DateTime? atUtc = null)
    {
        var response = await _http.PostAsJsonAsync("/api/discount-rules/simulate", new { MenuItemIds = menuItemIds, AtUtc = atUtc });
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<SimulationResult>(), null);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        return (null, problem?.Detail ?? "Erro ao simular desconto.");
    }

    private record ProblemDetails(string? Title, string? Detail);
}


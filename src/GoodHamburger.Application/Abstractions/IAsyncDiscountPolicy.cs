using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Application.Abstractions;

public record DiscountResult(decimal Percent, Guid? AppliedRuleId, string? AppliedRuleName);

public interface IAsyncDiscountPolicy
{
    Task<DiscountResult> CalculateAsync(
        IReadOnlyCollection<OrderItem> items,
        decimal subtotal,
        DateTime nowUtc,
        CancellationToken ct = default);
}

using GoodHamburger.Application.Abstractions;
using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Infrastructure.Services;

public class ParameterizedDiscountPolicy : IAsyncDiscountPolicy
{
    private readonly IDiscountRepository _discountRepository;

    public ParameterizedDiscountPolicy(IDiscountRepository discountRepository)
        => _discountRepository = discountRepository;

    public async Task<DiscountResult> CalculateAsync(
        IReadOnlyCollection<OrderItem> items,
        decimal subtotal,
        DateTime nowUtc,
        CancellationToken ct = default)
    {
        var rules = await _discountRepository.ListActiveOrderedByPriorityAsync(ct);

        foreach (var rule in rules)
        {
            if (rule.Matches(items, subtotal, nowUtc))
                return new DiscountResult(rule.Percent, rule.Id, rule.Name);
        }

        return new DiscountResult(0m, null, null);
    }
}

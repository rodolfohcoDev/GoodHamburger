using GoodHamburger.Domain.Entities;

namespace GoodHamburger.Domain.Services;

public interface IDiscountPolicy
{
    decimal CalculatePercent(IReadOnlyCollection<OrderItem> items);
}

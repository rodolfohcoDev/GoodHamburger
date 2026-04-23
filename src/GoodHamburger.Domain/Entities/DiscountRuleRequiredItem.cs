namespace GoodHamburger.Domain.Entities;

public class DiscountRuleRequiredItem
{
    public Guid DiscountRuleId { get; private set; }
    public Guid MenuItemId { get; private set; }

    // EF Core parameterless constructor
    private DiscountRuleRequiredItem() { }

    public DiscountRuleRequiredItem(Guid discountRuleId, Guid menuItemId)
    {
        DiscountRuleId = discountRuleId;
        MenuItemId = menuItemId;
    }
}

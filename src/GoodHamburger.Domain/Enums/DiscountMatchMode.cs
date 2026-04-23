namespace GoodHamburger.Domain.Enums;

public enum DiscountMatchMode
{
    /// <summary>Order must contain at least one item in each required category.</summary>
    CategoryAtLeast = 1,

    /// <summary>Order must contain exactly one item per required category and no extras.</summary>
    CategoryExact = 2,

    /// <summary>Order must contain exactly the specified menu item IDs (no more, no less).</summary>
    SpecificItems = 3
}

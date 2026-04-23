using GoodHamburger.Domain.Enums;
using GoodHamburger.Domain.Exceptions;

namespace GoodHamburger.Domain.Entities;

public class DiscountRule
{
    private List<DiscountRuleRequiredItem> _requiredItems = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Percent { get; private set; }
    public DiscountMatchMode MatchMode { get; private set; }
    public bool RequiresSandwich { get; private set; }
    public bool RequiresFries { get; private set; }
    public bool RequiresDrink { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public decimal? MinimumSubtotal { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }

    public IReadOnlyCollection<DiscountRuleRequiredItem> RequiredItems => _requiredItems.AsReadOnly();
    public IReadOnlyCollection<Guid> RequiredMenuItemIds =>
        _requiredItems.Select(x => x.MenuItemId).ToList().AsReadOnly();

    /// <summary>
    /// Fingerprint used for duplicate detection. Encodes match criteria so that two rules
    /// with identical criteria can be identified.
    /// </summary>
    public string Fingerprint =>
        $"{MatchMode}|{RequiresSandwich}|{RequiresFries}|{RequiresDrink}|" +
        string.Join(',', _requiredItems.Select(x => x.MenuItemId).OrderBy(x => x));

    // EF Core parameterless constructor
    private DiscountRule() { }

    public static DiscountRule Create(
        string name,
        decimal percent,
        DiscountMatchMode matchMode,
        bool requiresSandwich,
        bool requiresFries,
        bool requiresDrink,
        IReadOnlyCollection<Guid>? requiredMenuItemIds,
        int priority,
        bool isActive,
        decimal? minimumSubtotal,
        DateTime? validFrom,
        DateTime? validUntil)
    {
        var rule = new DiscountRule { Id = Guid.NewGuid(), IsActive = isActive };
        ValidateData(name, percent, matchMode, requiresSandwich, requiresFries, requiresDrink,
            requiredMenuItemIds ?? [], priority, minimumSubtotal, validFrom, validUntil);

        rule.Name = name;
        rule.Percent = percent;
        rule.MatchMode = matchMode;
        rule.RequiresSandwich = requiresSandwich;
        rule.RequiresFries = requiresFries;
        rule.RequiresDrink = requiresDrink;
        rule.Priority = priority;
        rule.MinimumSubtotal = minimumSubtotal;
        rule.ValidFrom = validFrom;
        rule.ValidUntil = validUntil;

        foreach (var id in requiredMenuItemIds ?? [])
            rule._requiredItems.Add(new DiscountRuleRequiredItem(rule.Id, id));

        return rule;
    }

    public void Update(
        string name,
        decimal percent,
        DiscountMatchMode matchMode,
        bool requiresSandwich,
        bool requiresFries,
        bool requiresDrink,
        IReadOnlyCollection<Guid>? requiredMenuItemIds,
        int priority,
        bool isActive,
        decimal? minimumSubtotal,
        DateTime? validFrom,
        DateTime? validUntil)
    {
        ValidateData(name, percent, matchMode, requiresSandwich, requiresFries, requiresDrink,
            requiredMenuItemIds ?? [], priority, minimumSubtotal, validFrom, validUntil);

        Name = name;
        Percent = percent;
        MatchMode = matchMode;
        RequiresSandwich = requiresSandwich;
        RequiresFries = requiresFries;
        RequiresDrink = requiresDrink;
        Priority = priority;
        IsActive = isActive;
        MinimumSubtotal = minimumSubtotal;
        ValidFrom = validFrom;
        ValidUntil = validUntil;

        _requiredItems.Clear();
        foreach (var id in requiredMenuItemIds ?? [])
            _requiredItems.Add(new DiscountRuleRequiredItem(Id, id));
    }

    public bool Matches(IReadOnlyCollection<OrderItem> items, decimal subtotal, DateTime nowUtc)
    {
        if (!IsActive) return false;
        if (ValidFrom.HasValue && nowUtc < ValidFrom.Value) return false;
        if (ValidUntil.HasValue && nowUtc >= ValidUntil.Value) return false;
        if (MinimumSubtotal.HasValue && subtotal < MinimumSubtotal.Value) return false;

        return MatchMode switch
        {
            DiscountMatchMode.CategoryAtLeast => MatchesCategoryAtLeast(items),
            DiscountMatchMode.CategoryExact   => MatchesCategoryExact(items),
            DiscountMatchMode.SpecificItems   => MatchesSpecificItems(items),
            _ => false
        };
    }

    private bool MatchesCategoryAtLeast(IReadOnlyCollection<OrderItem> items)
    {
        if (RequiresSandwich && !items.Any(i => i.Category == Enums.ProductCategory.Sandwich)) return false;
        if (RequiresFries    && !items.Any(i => i.Category == Enums.ProductCategory.Fries))    return false;
        if (RequiresDrink    && !items.Any(i => i.Category == Enums.ProductCategory.Drink))    return false;
        return true;
    }

    private bool MatchesCategoryExact(IReadOnlyCollection<OrderItem> items)
    {
        var requiredCount = (RequiresSandwich ? 1 : 0) + (RequiresFries ? 1 : 0) + (RequiresDrink ? 1 : 0);
        if (items.Count != requiredCount) return false;
        return MatchesCategoryAtLeast(items);
    }

    private bool MatchesSpecificItems(IReadOnlyCollection<OrderItem> items)
    {
        if (items.Count != _requiredItems.Count) return false;
        var orderIds  = items.Select(i => i.MenuItemId).OrderBy(x => x).ToList();
        var required  = _requiredItems.Select(x => x.MenuItemId).OrderBy(x => x).ToList();
        return orderIds.SequenceEqual(required);
    }

    private static void ValidateData(
        string name, decimal percent, DiscountMatchMode matchMode,
        bool requiresSandwich, bool requiresFries, bool requiresDrink,
        IReadOnlyCollection<Guid> requiredMenuItemIds,
        int priority, decimal? minimumSubtotal, DateTime? validFrom, DateTime? validUntil)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 80)
            throw new InvalidDiscountRuleException("Nome deve ter entre 1 e 80 caracteres.");

        if (percent <= 0 || percent > 100)
            throw new InvalidDiscountRuleException("Percentual deve ser entre 0,01 e 100.");

        if (priority < 1)
            throw new InvalidDiscountRuleException("Prioridade deve ser maior ou igual a 1.");

        if (minimumSubtotal.HasValue && minimumSubtotal.Value <= 0)
            throw new InvalidDiscountRuleException("Subtotal mínimo deve ser maior que zero.");

        if (validFrom.HasValue && validUntil.HasValue && validFrom.Value >= validUntil.Value)
            throw new InvalidDiscountRuleException("ValidFrom deve ser anterior a ValidUntil.");

        if (matchMode == DiscountMatchMode.SpecificItems)
        {
            if (requiredMenuItemIds.Count == 0)
                throw new InvalidDiscountRuleException("Modo SpecificItems requer ao menos um item.");
            if (requiredMenuItemIds.Distinct().Count() != requiredMenuItemIds.Count)
                throw new InvalidDiscountRuleException("IDs de itens duplicados.");
        }
        else
        {
            if (!requiresSandwich && !requiresFries && !requiresDrink)
                throw new InvalidDiscountRuleException("Ao menos uma categoria deve ser marcada.");
        }
    }
}

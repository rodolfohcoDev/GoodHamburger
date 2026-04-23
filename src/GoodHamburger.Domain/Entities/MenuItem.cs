using GoodHamburger.Domain.Enums;

namespace GoodHamburger.Domain.Entities;

public class MenuItem
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public ProductCategory Category { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MenuItem() { }

    public static MenuItem Create(string code, string name, decimal price, ProductCategory category)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Preço não pode ser negativo.", nameof(price));

        return new MenuItem
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Price = price,
            Category = category,
            IsActive = true
        };
    }

    public void Update(string name, decimal price, ProductCategory category, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(name));
        if (price < 0)
            throw new ArgumentException("Preço não pode ser negativo.", nameof(price));
        Name = name;
        Price = price;
        Category = category;
        IsActive = isActive;
    }
}

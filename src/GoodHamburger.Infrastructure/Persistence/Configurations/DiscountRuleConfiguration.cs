using GoodHamburger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoodHamburger.Infrastructure.Persistence.Configurations;

public class DiscountRuleConfiguration : IEntityTypeConfiguration<DiscountRule>
{
    public void Configure(EntityTypeBuilder<DiscountRule> builder)
    {
        builder.ToTable("discount_rules");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(80);
        builder.Property(r => r.Percent).HasColumnType("decimal(5,2)").IsRequired();
        builder.Property(r => r.MatchMode).HasConversion<int>().IsRequired();
        builder.Property(r => r.RequiresSandwich).IsRequired();
        builder.Property(r => r.RequiresFries).IsRequired();
        builder.Property(r => r.RequiresDrink).IsRequired();
        builder.Property(r => r.Priority).IsRequired();
        builder.Property(r => r.IsActive).IsRequired();
        builder.Property(r => r.MinimumSubtotal).HasColumnType("decimal(10,2)");
        builder.Property(r => r.ValidFrom);
        builder.Property(r => r.ValidUntil);

        builder.Ignore(r => r.RequiredMenuItemIds);
        builder.Ignore(r => r.Fingerprint);

        builder.HasMany(r => r.RequiredItems)
            .WithOne()
            .HasForeignKey(x => x.DiscountRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.RequiredItems).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class DiscountRuleRequiredItemConfiguration : IEntityTypeConfiguration<DiscountRuleRequiredItem>
{
    public void Configure(EntityTypeBuilder<DiscountRuleRequiredItem> builder)
    {
        builder.ToTable("discount_rule_required_items");
        builder.HasKey(x => new { x.DiscountRuleId, x.MenuItemId });
    }
}

using GoodHamburger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoodHamburger.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CreatedAt).IsRequired();

        builder.Property(o => o.Subtotal)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.DiscountPercent)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.DiscountAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.Total)
            .HasColumnType("decimal(10,2)");

        builder.HasMany<OrderItem>(o => o.Items)
            .WithOne()
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

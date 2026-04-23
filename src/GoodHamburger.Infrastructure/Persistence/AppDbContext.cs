using GoodHamburger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoodHamburger.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<DiscountRule> DiscountRules => Set<DiscountRule>();
    public DbSet<DiscountRuleRequiredItem> DiscountRuleRequiredItems => Set<DiscountRuleRequiredItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.DbContexts.Interfaces;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class OrderSinkDbContext : TeddySwapSinkDbContext, ITeddySwapDbContext
{

    #region TeddySwap Models
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Price> Prices => Set<Price>();
    public DbSet<AddressVerification> AddressVerifications => Set<AddressVerification>();
    public DbSet<BlacklistedAddress> BlacklistedAddresses => Set<BlacklistedAddress>();
    #endregion

    public OrderSinkDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().HasKey(order => new { order.TxHash, order.Index });
        modelBuilder.Entity<Price>().HasKey(price => new { price.TxHash, price.Index });
        modelBuilder.Entity<BlacklistedAddress>().HasKey(ba => ba.Address);
        modelBuilder.Entity<AddressVerification>().HasKey(a => a.TestnetAddress);

        // Relations

        modelBuilder.Entity<Price>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Price)
            .HasForeignKey<Price>(p => new { p.TxHash, p.Index });

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Slot)
            .IsUnique(false);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.UserAddress)
            .IsUnique(false);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.BatcherAddress)
            .IsUnique(false);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderType)
            .IsUnique(false);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Slot)
            .IsUnique(false);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Slot)
            .IsUnique(false);

        base.OnModelCreating(modelBuilder);
    }
}
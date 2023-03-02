using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class TeddySwapSinkDbContext : DbContext
{

    #region Core Models
    public DbSet<TxOutput> TxOutputs => Set<TxOutput>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Asset> Assets => Set<Asset>();
    #endregion

    #region TeddySwap Models
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Price> Prices => Set<Price>();
    public DbSet<AddressVerification> AddressVerifications => Set<AddressVerification>();
    public DbSet<BlacklistedAddress> BlacklistedAddresses => Set<BlacklistedAddress>();

    #endregion

    public TeddySwapSinkDbContext(DbContextOptions<TeddySwapSinkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<Asset>().HasKey(asset => new { asset.PolicyId, asset.Name, asset.TxOutputHash, asset.TxOutputIndex });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<Order>().HasKey(order => new { order.TxHash, order.Index });
        modelBuilder.Entity<Price>().HasKey(price => new { price.TxHash, price.Index });
        modelBuilder.Entity<BlacklistedAddress>().HasKey(ba => ba.Address);
        modelBuilder.Entity<AddressVerification>().HasKey(a => a.TestnetAddress);
        modelBuilder.Entity<Transaction>().HasKey(tx => new { tx.Hash, tx.Index });
        modelBuilder.Entity<Block>().Property(block => block.InvalidTransactions).HasColumnType("jsonb");

        // Relations
        modelBuilder.Entity<Block>()
            .HasMany(b => b.Orders)
            .WithOne(o => o.Block)
            .HasForeignKey(o => o.Blockhash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Block>()
            .HasMany(b => b.Transactions)
            .WithOne(t => t.Block)
            .HasForeignKey(b => b.Blockhash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Block)
            .WithMany(b => b.Transactions)
            .HasForeignKey(t => t.Blockhash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasOne<Block>(o => o.Block)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.Blockhash)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Price>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Price)
            .HasForeignKey<Price>(p => new { p.TxHash, p.Index });

        modelBuilder.Entity<TxOutput>()
            .HasOne<Transaction>(txOutput => txOutput.Transaction)
            .WithMany(tx => tx.Outputs)
            .HasForeignKey(txOutput => new { txOutput.TxHash, txOutput.TxIndex });

        modelBuilder.Entity<Asset>()
            .HasOne<TxOutput>(asset => asset.TxOutput)
            .WithMany(txOutput => txOutput.Assets)
            .HasForeignKey(asset => new { asset.TxOutputHash, asset.TxOutputIndex });

        base.OnModelCreating(modelBuilder);
    }
}
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

    public TeddySwapSinkDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<Asset>().HasKey(asset => new { asset.PolicyId, asset.Name, asset.TxOutputHash, asset.TxOutputIndex });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<Transaction>().HasKey(tx => new { tx.Hash, tx.Index });
        modelBuilder.Entity<Block>().Property(block => block.InvalidTransactions).HasColumnType("jsonb");
        modelBuilder.Entity<Transaction>().Property(tx => tx.Metadata).HasColumnType("jsonb");

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
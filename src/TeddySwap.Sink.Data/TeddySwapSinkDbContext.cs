using TeddySwap.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace TeddySwap.Sink.Data;

public class TeddySwapSinkDbContext : DbContext
{

    #region Core Models
    public DbSet<TxInput> TxInputs => Set<TxInput>();
    public DbSet<TxOutput> TxOutputs => Set<TxOutput>();
    public DbSet<CollateralTxInput> CollateralTxInputs => Set<CollateralTxInput>();
    public DbSet<CollateralTxOutput> CollateralTxOutputs => Set<CollateralTxOutput>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Asset> Assets => Set<Asset>();
    #endregion

    public TeddySwapSinkDbContext(DbContextOptions<TeddySwapSinkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<TxInput>().HasKey(txInput => new { txInput.TxHash, txInput.TxOutputHash, txInput.TxOutputIndex });
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<CollateralTxInput>().HasKey(txInput => new { txInput.TxHash, txInput.TxOutputHash, txInput.TxOutputIndex });
        modelBuilder.Entity<CollateralTxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<Asset>().HasKey(asset => new { asset.PolicyId, asset.Name, asset.TxOutputHash, asset.TxOutputIndex });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<Transaction>().HasKey(tx => tx.Hash);
        modelBuilder.Entity<Block>().Property(block => block.InvalidTransactions).HasColumnType("jsonb");
        // Relations
        modelBuilder.Entity<TxInput>()
            .HasOne<TxOutput>(txInput => txInput.TxOutput)
            .WithMany(txOutput => txOutput.Inputs)
            .HasForeignKey(txInput => new { txInput.TxOutputHash, txInput.TxOutputIndex });

        modelBuilder.Entity<TxInput>()
            .HasOne<Transaction>(txInput => txInput.Transaction)
            .WithMany(tx => tx.Inputs)
            .HasForeignKey(txInput => txInput.TxHash);

        modelBuilder.Entity<CollateralTxInput>()
            .HasOne<TxOutput>(txInput => txInput.TxOutput)
            .WithMany(txOutput => txOutput.CollateralInputs)
            .HasForeignKey(txInput => new { txInput.TxOutputHash, txInput.TxOutputIndex });

        modelBuilder.Entity<CollateralTxInput>()
            .HasOne<Transaction>(txInput => txInput.Transaction)
            .WithMany(tx => tx.CollateralInputs)
            .HasForeignKey(txInput => txInput.TxHash);

        modelBuilder.Entity<TxOutput>()
            .HasOne<Transaction>(txOutput => txOutput.Transaction)
            .WithMany(tx => tx.Outputs)
            .HasForeignKey(txOutput => txOutput.TxHash);

        modelBuilder.Entity<CollateralTxOutput>()
            .HasOne<Transaction>(txOutput => txOutput.Transaction)
            .WithOne(tx => tx.CollateralOutput)
            .HasForeignKey<CollateralTxOutput>(txOut => txOut.TxHash);

        modelBuilder.Entity<Asset>()
            .HasOne<TxOutput>(asset => asset.TxOutput)
            .WithMany(txOutput => txOutput.Assets)
            .HasForeignKey(asset => new { asset.TxOutputHash, asset.TxOutputIndex });

        base.OnModelCreating(modelBuilder);
    }
}
using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class TeddySwapSinkCoreDbContext : DbContext
{
    public DbSet<TxInput> TxInputs => Set<TxInput>();
    public DbSet<TxOutput> TxOutputs => Set<TxOutput>();
    public DbSet<CollateralTxIn> CollateralTxIns => Set<CollateralTxIn>();
    public DbSet<CollateralTxOut> CollateralTxOuts => Set<CollateralTxOut>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Asset> Assets => Set<Asset>();

    public TeddySwapSinkCoreDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<TxInput>().HasKey(txInput => new { txInput.TxHash, txInput.TxOutputHash, txInput.TxOutputIndex });
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<CollateralTxOut>().HasKey(txOut => new { txOut.Address, txOut.TxHash });
        modelBuilder.Entity<CollateralTxIn>().HasKey(txInput => new { txInput.TxHash, txInput.TxOutputHash, txInput.TxOutputIndex });
        modelBuilder.Entity<Asset>().HasKey(asset => new { asset.PolicyId, asset.Name, asset.TxOutputHash, asset.TxOutputIndex });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<Transaction>().HasKey(tx => tx.Hash);
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
            .HasForeignKey(t => t.Blockhash);

        modelBuilder.Entity<Transaction>()
            .HasMany(t => t.Inputs)
            .WithOne(i => i.Transaction)
            .HasForeignKey(i => i.TxHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Transaction>()
            .HasMany(t => t.Outputs)
            .WithOne(o => o.Transaction)
            .HasForeignKey(o => o.TxHash)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TxInput>()
            .HasOne<TxOutput>(txInput => txInput.TxOutput)
            .WithMany(txOutput => txOutput.Inputs)
            .HasForeignKey(txInput => new { txInput.TxOutputHash, txInput.TxOutputIndex });

        modelBuilder.Entity<TxInput>()
            .HasOne<Transaction>(txInput => txInput.Transaction)
            .WithMany(tx => tx.Inputs)
            .HasForeignKey(txInput => txInput.TxHash);

        modelBuilder.Entity<TxOutput>()
            .HasOne<Transaction>(txOutput => txOutput.Transaction)
            .WithMany(tx => tx.Outputs)
            .HasForeignKey(txOutput => txOutput.TxHash);

        modelBuilder.Entity<CollateralTxIn>()
            .HasOne<Transaction>(txInput => txInput.Transaction)
            .WithMany(tx => tx.CollateralTxIns)
            .HasForeignKey(txInput => txInput.TxHash);

        modelBuilder.Entity<Asset>()
            .HasOne<TxOutput>(asset => asset.TxOutput)
            .WithMany(txOutput => txOutput.Assets)
            .HasForeignKey(asset => new { asset.TxOutputHash, asset.TxOutputIndex });

        modelBuilder.Entity<CollateralTxOut>()
            .HasOne<Transaction>(txOutput => txOutput.Transaction)
            .WithOne(tx => tx.CollateralTxOut)
            .HasForeignKey<CollateralTxOut>(txOut => txOut.TxHash);

        base.OnModelCreating(modelBuilder);
    }
}
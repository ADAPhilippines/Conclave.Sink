
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Data;

public class ConclaveSinkDbContext : DbContext
{
    public DbSet<AddressByStake> AddressByStake => Set<AddressByStake>();
    public DbSet<BalanceByAddress> BalanceByAddress => Set<BalanceByAddress>();
    public DbSet<BalanceByStakeEpoch> BalanceByStakeEpoch => Set<BalanceByStakeEpoch>();
    public DbSet<WithdrawalByStakeEpoch> WithdrawalByStakeEpoch => Set<WithdrawalByStakeEpoch>();
    public DbSet<CnclvByStakeEpoch> CnclvByStakeEpoch => Set<CnclvByStakeEpoch>();

    #region Core Models
    public DbSet<TxInput> TxInputs => Set<TxInput>();
    public DbSet<TxOutput> TxOutputs => Set<TxOutput>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<PoolRegistration> PoolRegistrations => Set<PoolRegistration>();
    public DbSet<PoolRetirement> PoolRetirements => Set<PoolRetirement>();
    public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();
    public DbSet<StakeDelegation> StakeDelegations => Set<StakeDelegation>();
    #endregion

    public ConclaveSinkDbContext(DbContextOptions<ConclaveSinkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<AddressByStake>().HasKey(s => s.StakeAddress);
        modelBuilder.Entity<BalanceByAddress>().HasKey(s => s.Address);
        modelBuilder.Entity<TxInput>().HasKey(txInput => new { txInput.TxHash, txInput.TxOutputHash, txInput.TxOutputIndex });
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<Asset>().HasKey(asset => new { asset.PolicyId, asset.Name, asset.TxOutputHash, asset.TxOutputIndex });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<BalanceByStakeEpoch>().HasKey(s => new { s.StakeAddress, s.Epoch });
        modelBuilder.Entity<PoolRegistration>().HasKey(prg => new { prg.PoolId, prg.TxHash });
        modelBuilder.Entity<PoolRegistration>().Property(prg => prg.PoolMetadataJSON).HasColumnType("jsonb");
        modelBuilder.Entity<PoolRetirement>().HasKey(prt => new { prt.Pool, prt.TxHash });
        modelBuilder.Entity<WithdrawalByStakeEpoch>().HasKey(wbse => new { wbse.StakeAddress, wbse.Epoch });
        modelBuilder.Entity<StakeDelegation>().HasKey(sd => new { sd.StakeAddress, sd.TxHash });
        modelBuilder.Entity<Transaction>().HasKey(tx => tx.Hash);
        modelBuilder.Entity<Withdrawal>().HasKey(w => new { w.TxHash, w.StakeAddress });
        modelBuilder.Entity<Transaction>().Property(b => b.Withdrawals).HasColumnType("jsonb");
        modelBuilder.Entity<CnclvByStakeEpoch>().HasKey(s => new { s.StakeAddress, s.Epoch });

        // Relations
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

        modelBuilder.Entity<Asset>()
            .HasOne<TxOutput>(asset => asset.TxOutput)
            .WithMany(txOutput => txOutput.Assets)
            .HasForeignKey(asset => new { asset.TxOutputHash, asset.TxOutputIndex });

        modelBuilder.Entity<Withdrawal>()
            .HasOne<Transaction>(withdrawal => withdrawal.Transaction)
            .WithMany(tx => tx.Withdrawals)
            .HasForeignKey(withdrawal => withdrawal.TxHash);

        modelBuilder.Entity<StakeDelegation>()
            .HasOne<Transaction>(stakeDelegation => stakeDelegation.Transaction)
            .WithMany(tx => tx.StakeDelegations)
            .HasForeignKey(stakeDelegation => stakeDelegation.TxHash);

        base.OnModelCreating(modelBuilder);
    }
}
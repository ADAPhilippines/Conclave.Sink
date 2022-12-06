
using Conclave.Sink.Models;
using Microsoft.EntityFrameworkCore;

namespace Conclave.Sink.Data;

public class ConclaveSinkDbContext : DbContext
{
    public DbSet<AddressByStake> AddressByStake => Set<AddressByStake>();
    public DbSet<BalanceByAddress> BalanceByAddress => Set<BalanceByAddress>();
    public DbSet<TxInput> TxInput => Set<TxInput>();
    public DbSet<TxOutput> TxOutput => Set<TxOutput>();
    public DbSet<Block> Block => Set<Block>();
    public DbSet<BalanceByStakeAddressEpoch> BalanceByStakeAddressEpoch => Set<BalanceByStakeAddressEpoch>();
    public DbSet<PoolRegistration> PoolRegistration => Set<PoolRegistration>();
    public DbSet<PoolRetirement> PoolRetirement => Set<PoolRetirement>();
    public DbSet<WithdrawalByStakeEpoch> WithdrawalByStakeEpoch => Set<WithdrawalByStakeEpoch>();
    public DbSet<StakeByPoolEpoch> StakeByPoolEpoch => Set<StakeByPoolEpoch>();
    public DbSet<Transaction> Transaction => Set<Transaction>();

    public ConclaveSinkDbContext(DbContextOptions<ConclaveSinkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primary Keys
        modelBuilder.Entity<AddressByStake>().HasKey(s => s.StakeAddress);
        modelBuilder.Entity<BalanceByAddress>().HasKey(s => s.Address);
        modelBuilder.Entity<TxInput>().HasKey(txInput => new { txInput.TxHash, txInput.TxOutputHash, txInput.TxOutputIndex });
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<BalanceByStakeAddressEpoch>().HasKey(s => new { s.StakeAddress, s.Epoch });
        modelBuilder.Entity<PoolRegistration>().HasKey(prg => new { prg.PoolId, prg.TxHash });
        modelBuilder.Entity<PoolRegistration>().Property(prg => prg.PoolMetadataJSON).HasColumnType("jsonb");
        modelBuilder.Entity<PoolRetirement>().HasKey(prt => new { prt.Pool, prt.TxHash });
        modelBuilder.Entity<WithdrawalByStakeEpoch>().HasKey(wbse => new { wbse.StakeAddress, wbse.Epoch });
        modelBuilder.Entity<StakeByPoolEpoch>().HasKey(de => new { de.StakeAddress, de.PoolId, de.TxHash, de.TxIndex });
        modelBuilder.Entity<Transaction>().HasKey(tx => tx.Hash);
        modelBuilder.Entity<Transaction>().Property(b => b.Withdrawals).HasColumnType("jsonb");

        // Relations
        modelBuilder.Entity<TxInput>()
            .HasOne<TxOutput>(txInput => txInput.TxOutput)
            .WithMany(txOutput => txOutput.Inputs)
            .HasForeignKey(txInput => new { txInput.TxOutputHash, txInput.TxOutputIndex });

        modelBuilder.Entity<PoolRegistration>()
            .HasOne(pool => pool.Block)
            .WithMany(block => block.PoolRegistrations)
            .HasForeignKey(pool => pool.BlockHash)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}

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
    public DbSet<DelegatorByEpoch> DelegatorByEpoch => Set<DelegatorByEpoch>();
    public DbSet<RewardAddressByPoolPerEpoch> RewardAddressByPoolPerEpoch => Set<RewardAddressByPoolPerEpoch>();

    public ConclaveSinkDbContext(DbContextOptions<ConclaveSinkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AddressByStake>().HasKey(s => s.StakeAddress);
        modelBuilder.Entity<BalanceByAddress>().HasKey(s => s.Address);
        modelBuilder.Entity<TxInput>().HasKey(txInput => new { txInput.TxHash, txInput.TxInputOutputHash, txInput.TxInputOutputIndex, txInput.Slot });
        modelBuilder.Entity<TxOutput>().HasKey(txOut => new { txOut.TxHash, txOut.Index });
        modelBuilder.Entity<Block>().HasKey(block => block.BlockHash);
        modelBuilder.Entity<DelegatorByEpoch>().HasKey(de => new { de.StakeAddress, de.PoolId, de.TxHash, de.TxIndex });
        modelBuilder.Entity<RewardAddressByPoolPerEpoch>().HasKey(rabppe => new { rabppe.PoolId, rabppe.RewardAddress, rabppe.TxHash, rabppe.TxIndex });
        base.OnModelCreating(modelBuilder);
    }
}
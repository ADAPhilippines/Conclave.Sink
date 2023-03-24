using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class TeddySwapFisoSinkDbContext : TeddySwapSinkCoreDbContext
{

    #region TeddySwap Models
    public DbSet<Delegator> FisoDelegators => Set<Delegator>();
    public DbSet<FisoBonusDelegation> FisoBonusDelegations => Set<FisoBonusDelegation>();
    public DbSet<FisoEpochReward> FisoEpochRewards => Set<FisoEpochReward>();
    public DbSet<FisoPoolActiveStake> FisoPoolActiveStakes => Set<FisoPoolActiveStake>();
    public DbSet<BalanceByStakeEpoch> BalanceByStakeEpoch => Set<BalanceByStakeEpoch>();
    #endregion

    public TeddySwapFisoSinkDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FisoBonusDelegation>().HasKey(fbd => new { fbd.EpochNumber, fbd.PoolId, fbd.StakeAddress, fbd.TxHash });
        modelBuilder.Entity<FisoEpochReward>().HasKey(fer => new { fer.EpochNumber, fer.StakeAddress });
        modelBuilder.Entity<FisoPoolActiveStake>().HasKey(fpas => new { fpas.EpochNumber, fpas.PoolId });
        modelBuilder.Entity<Delegator>().HasKey(d => new { d.StakeAddress, d.PoolId, d.Epoch });
        modelBuilder.Entity<BalanceByStakeEpoch>().HasKey(s => new { s.StakeAddress, s.Epoch });
        base.OnModelCreating(modelBuilder);
    }
}
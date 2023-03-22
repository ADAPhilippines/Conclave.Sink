using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class TeddySwapFisoSinkDbContext : TeddySwapSinkCoreDbContext
{

    #region TeddySwap Models
    public DbSet<FisoBonusDelegation> FisoBonusDelegations => Set<FisoBonusDelegation>();
    public DbSet<FisoEpochReward> FisoEpochRewards => Set<FisoEpochReward>();
    public DbSet<FisoPoolActiveStake> FisoPoolActiveStakes => Set<FisoPoolActiveStake>();
    #endregion

    public TeddySwapFisoSinkDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FisoBonusDelegation>().HasKey(fbd => new { fbd.EpochNumber, fbd.PoolId, fbd.StakeAddress, fbd.TxHash });
        modelBuilder.Entity<FisoEpochReward>().HasKey(fer => new { fer.EpochNumber, fer.StakeAddress });
        modelBuilder.Entity<FisoPoolActiveStake>().HasKey(fpas => new { fpas.EpochNumber, fpas.PoolId });
        base.OnModelCreating(modelBuilder);
    }
}
using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class TeddySwapFisoSinkDbContext : TeddySwapSinkCoreDbContext
{

    #region TeddySwap Models
    public DbSet<FisoDelegator> FisoDelegators => Set<FisoDelegator>();
    public DbSet<FisoBonusDelegation> FisoBonusDelegations => Set<FisoBonusDelegation>();
    public DbSet<FisoEpochReward> FisoEpochRewards => Set<FisoEpochReward>();
    public DbSet<FisoPoolActiveStake> FisoPoolActiveStakes => Set<FisoPoolActiveStake>();
    #endregion

    public TeddySwapFisoSinkDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FisoBonusDelegation>().HasKey(fbd => new { fbd.StakeAddress, fbd.TxHash, fbd.Slot });
        modelBuilder.Entity<FisoEpochReward>().HasKey(fer => new { fer.EpochNumber, fer.StakeAddress });
        modelBuilder.Entity<FisoPoolActiveStake>().HasKey(fpas => new { fpas.EpochNumber, fpas.PoolId });
        modelBuilder.Entity<FisoDelegator>().HasKey(d => new { d.StakeAddress, d.PoolId, d.Epoch });
        base.OnModelCreating(modelBuilder);
    }
}
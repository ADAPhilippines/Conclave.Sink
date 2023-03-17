using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Sink.Data;

public class TeddySwapNftSinkDbContext : TeddySwapSinkDbContext
{

    #region TeddySwap Models
    public DbSet<Nft> Nfts => Set<Nft>();
    public DbSet<NftOwner> NftOwners => Set<NftOwner>();
    #endregion

    public TeddySwapNftSinkDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Nft>().HasKey(nft => new { nft.PolicyId, nft.TokenName });
        modelBuilder.Entity<Nft>().Property(nft => nft.Metadata).HasColumnType("jsonb");

        base.OnModelCreating(modelBuilder);
    }
}
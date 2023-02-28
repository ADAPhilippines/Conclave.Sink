using TeddySwap.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace TeddySwap.Sink.Data;

public class ConclaveSinkDbContext : DbContext
{
    public ConclaveSinkDbContext(DbContextOptions<ConclaveSinkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
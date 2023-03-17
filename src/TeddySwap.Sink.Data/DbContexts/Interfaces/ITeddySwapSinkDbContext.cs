using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models;

namespace TeddySwap.Common.DbContexts.Interfaces;

public interface ITeddySwapDbContext
{
    DbSet<TxOutput> TxOutputs { get; }
    DbSet<Block> Blocks { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<Asset> Assets { get; }
}
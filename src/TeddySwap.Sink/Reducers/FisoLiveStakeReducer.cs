using System.Text.Json;
using CardanoSharp.Koios.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeddySwap.Common.Models;
using TeddySwap.Sink.Data;
using TeddySwap.Sink.Models;
using TeddySwap.Sink.Models.Oura;
using TeddySwap.Sink.Services;

namespace TeddySwap.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput, OuraVariant.TxOutput, OuraVariant.CollateralInput, OuraVariant.CollateralOutput)]
public class FisoLiveStakeReducer : OuraReducerBase
{
    private readonly ILogger<FisoLiveStakeReducer> _logger;
    private readonly IDbContextFactory<TeddySwapFisoSinkDbContext> _dbContextFactory;
    private readonly IDbContextFactory<CardanoDbSyncContext> _cardanoDbSyncContextFactory;
    private readonly CardanoService _cardanoService;
    private readonly IPoolClient _poolClient;
    private readonly ITransactionClient _transactionClient;
    private readonly TeddySwapSinkSettings _settings;

    public FisoLiveStakeReducer(
        ILogger<FisoLiveStakeReducer> logger,
        IDbContextFactory<TeddySwapFisoSinkDbContext> dbContextFactory,
        IOptions<TeddySwapSinkSettings> settings,
        CardanoService cardanoService,
        IPoolClient poolClient,
        ITransactionClient transactionClient,
        IDbContextFactory<CardanoDbSyncContext> cardanoDbSyncContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _poolClient = poolClient;
        _settings = settings.Value;
        _transactionClient = transactionClient;
        _cardanoDbSyncContextFactory = cardanoDbSyncContextFactory;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        if (ouraEvent.Context is null || ouraEvent.Context.Slot is null) return;

        ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)ouraEvent.Context.Slot);

        if (epoch < _settings.FisoStartEpoch - 1 || epoch >= _settings.FisoEndEpoch) return;

        using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        await (ouraEvent.Variant switch
        {
            OuraVariant.TxInput => Task.Run(async () =>
            {
                OuraTxInput? txInput = ouraEvent as OuraTxInput;
                if (txInput is not null &&
                    txInput.Context is not null &&
                    txInput.Context.Slot is not null &&
                    txInput.Context.TxIdx is not null &&
                    txInput.Context.TxHash is not null)
                {
                    if (txInput.Context.InvalidTransactions is not null &&
                        txInput.Context.InvalidTransactions.ToList().Contains((ulong)txInput.Context.TxIdx)) return;

                    TxOutput? input = await _dbContext.TxOutputs
                        .Include(txOut => txOut.Transaction)
                        .ThenInclude(transaction => transaction.Block)
                        .Where(txOut => txOut.TxHash == txInput.TxHash && txOut.Index == txInput.Index)
                        .FirstOrDefaultAsync();

                    if (input is null)
                    {
                        var txOut = await GetTxOut(txInput.TxHash, (int)txInput.Index);

                        if (txOut is null) return;

                        input = new TxOutput()
                        {
                            Address = txOut.Address,
                            Amount = (ulong)txOut.Value,
                        };
                    }

                    if (input is not null)
                    {
                        string? stakeAddress = _cardanoService.TryGetStakeAddress(input.Address);
                        if (stakeAddress is not null)
                        {
                            var fisoDelegator = await _dbContext.FisoDelegators.Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch).FirstOrDefaultAsync();

                            if (fisoDelegator is not null)
                            {
                                var fisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                                    .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
                                    .FirstOrDefaultAsync();

                                if (fisoPoolActiveStake is not null)
                                {
                                    fisoPoolActiveStake.StakeAmount -= input.Amount;
                                    _dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);
                                }
                            }
                        }
                    }
                    await _dbContext.SaveChangesAsync();

                }
            }),
            OuraVariant.TxOutput => Task.Run(async () =>
            {
                OuraTxOutput? txOutput = ouraEvent as OuraTxOutput;
                if (txOutput is not null &&
                    txOutput.Amount is not null &&
                    txOutput.Address is not null &&
                    txOutput.Context is not null &&
                    txOutput.Context.Slot is not null &&
                    txOutput.Context.TxIdx is not null)
                {
                    if (txOutput.Context.InvalidTransactions is not null &&
                        txOutput.Context.InvalidTransactions.ToList().Contains((ulong)txOutput.Context.TxIdx)) return;

                    string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutput.Address);
                    ulong amount = (ulong)txOutput.Amount;


                    var fisoDelegator = await _dbContext.FisoDelegators.Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch).FirstOrDefaultAsync();

                    if (fisoDelegator is not null)
                    {
                        var fisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                            .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
                            .FirstOrDefaultAsync();

                        if (fisoPoolActiveStake is not null)
                        {
                            fisoPoolActiveStake.StakeAmount += amount;
                            _dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }),
            OuraVariant.CollateralInput => Task.Run(async () =>
            {
                OuraTxInput? txInput = ouraEvent as OuraTxInput;

                if (txInput is not null &&
                    txInput.Context is not null &&
                    txInput.Context.Slot is not null &&
                    txInput.Context.TxIdx is not null)
                {
                    if (txInput.Context.HasCollateralOutput && txInput.Context.TxHash is not null)
                    {
                        TxOutput? input = await _dbContext.TxOutputs
                            .Include(txOut => txOut.Transaction)
                            .ThenInclude(transaction => transaction.Block)
                            .Where(txOut => txOut.TxHash == txInput.TxHash && txOut.Index == txInput.Index)
                            .FirstOrDefaultAsync();

                        if (input is null)
                        {
                            var txOut = await GetTxOut(txInput.TxHash, (int)txInput.Index);

                            if (txOut is null) return;

                            input = new TxOutput()
                            {
                                Address = txOut.Address,
                                Amount = (ulong)txOut.Value,
                            };
                        }

                        if (input is not null)
                        {
                            string? stakeAddress = _cardanoService.TryGetStakeAddress(input.Address);
                            if (stakeAddress is not null)
                            {
                                var fisoDelegator = await _dbContext.FisoDelegators.Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch).FirstOrDefaultAsync();

                                if (fisoDelegator is not null)
                                {
                                    var fisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                                        .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
                                        .FirstOrDefaultAsync();

                                    if (fisoPoolActiveStake is not null)
                                    {
                                        fisoPoolActiveStake.StakeAmount -= input.Amount;
                                        _dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);
                                    }
                                }
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }),
            OuraVariant.CollateralOutput => Task.Run(async () =>
            {
                OuraCollateralOutput? txOutput = ouraEvent as OuraCollateralOutput;
                if (txOutput is not null &&
                    txOutput.Context is not null &&
                    txOutput.Context.HasCollateralOutput &&
                    txOutput.Context.Slot is not null &&
                    txOutput.Address is not null)
                {
                    string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutput.Address);
                    ulong amount = txOutput.Amount;

                    var fisoDelegator = await _dbContext.FisoDelegators.Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch).FirstOrDefaultAsync();

                    if (fisoDelegator is not null)
                    {
                        var fisoPoolActiveStake = await _dbContext.FisoPoolActiveStakes
                            .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
                            .FirstOrDefaultAsync();

                        if (fisoPoolActiveStake is not null)
                        {
                            fisoPoolActiveStake.StakeAmount += amount;
                            _dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }),
            _ => Task.Run(() => { })
        });
    }

    public async Task<Common.Models.CardanoDbSync.TxOut?> GetTxOut(string hash, int index)
    {
        using CardanoDbSyncContext _dbContext = await _cardanoDbSyncContextFactory.CreateDbContextAsync();
        byte[] txHashBytes = Convert.FromHexString(hash);
        Common.Models.CardanoDbSync.TxOut? txOut = await _dbContext.TxOuts
            .Include(to => to.Tx)
            .Where(to => to.Tx.Hash == txHashBytes && to.Index == index)
            .FirstOrDefaultAsync();

        return txOut;
    }

    public async Task<TxOutput?> ResolveTxOutAsync(string hash, ulong index, TeddySwapFisoSinkDbContext dbContext)
    {
        TxOutput? txInOut = await dbContext.TxOutputs
            .Include(txOut => txOut.Transaction)
            .ThenInclude(transaction => transaction.Block)
            .Where(txOut => txOut.TxHash == hash && txOut.Index == index)
            .FirstOrDefaultAsync();

        if (txInOut is null)
        {
            var txOut = await GetTxOut(hash, (int)index);

            if (txOut is null) return null;

            txInOut = new TxOutput()
            {
                Address = txOut.Address,
                Amount = (ulong)txOut.Value,
            };
        }

        return txInOut;
    }

    public async Task UpdateFisoPoolAsync(string address, ulong epoch, ulong amount, TeddySwapFisoSinkDbContext dbContext)
    {
        string? stakeAddress = _cardanoService.TryGetStakeAddress(address);

        if (stakeAddress is null) return;

        var fisoDelegator = await dbContext.FisoDelegators
            .Where(fd => fd.StakeAddress == stakeAddress && fd.Epoch == epoch)
            .FirstOrDefaultAsync();

        if (fisoDelegator is null) return;

        var fisoPoolActiveStake = await dbContext.FisoPoolActiveStakes
            .Where(fpas => fpas.PoolId == fisoDelegator.PoolId && fpas.EpochNumber == epoch)
            .FirstOrDefaultAsync();

        if (fisoPoolActiveStake is null) return;

        fisoPoolActiveStake.StakeAmount += amount;
        dbContext.FisoPoolActiveStakes.Update(fisoPoolActiveStake);
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        using TeddySwapFisoSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<Transaction> transactions = await _dbContext.Transactions
            .Include(tx => tx.Inputs)
            .Include(tx => tx.Outputs)
            .Include(tx => tx.CollateralTxIns)
            .Include(tx => tx.CollateralTxOut)
            .Include(tx => tx.Block)
            .Where(tx => tx.Block == rollbackBlock)
            .ToListAsync();

        if (!transactions.Any()) return;

        foreach (Transaction transaction in transactions)
        {
            ulong epoch = transaction.Block.Epoch;

            if (!(transaction.Block.InvalidTransactions is not null &&
                transaction.Block.InvalidTransactions.ToList().Contains(transaction.Index)))
            {
                foreach (TxInput input in transaction.Inputs)
                {
                    TxOutput? txInOut = await ResolveTxOutAsync(input.TxHash, input.TxOutputIndex, _dbContext);

                    if (txInOut is null) continue;

                    await UpdateFisoPoolAsync(txInOut.Address, epoch, txInOut.Amount, _dbContext);

                }

                foreach (TxOutput output in transaction.Outputs) await UpdateFisoPoolAsync(output.Address, epoch, output.Amount, _dbContext);
            }
            else
            {
                foreach (CollateralTxIn collateralTxIn in transaction.CollateralTxIns)
                {
                    TxOutput? txInOut = await ResolveTxOutAsync(collateralTxIn.TxHash, collateralTxIn.TxOutputIndex, _dbContext);

                    if (txInOut is null) continue;

                    await UpdateFisoPoolAsync(txInOut.Address, epoch, txInOut.Amount, _dbContext);
                }

                if (transaction.CollateralTxOut is not null)
                    await UpdateFisoPoolAsync(transaction.CollateralTxOut.Address, epoch, transaction.CollateralTxOut.Amount, _dbContext);
            }
        }
    }
}
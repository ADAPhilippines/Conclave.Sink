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
        ITransactionClient transactionClient
        )
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _cardanoService = cardanoService;
        _poolClient = poolClient;
        _settings = settings.Value;
        _transactionClient = transactionClient;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
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

                    ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txInput.Context.Slot);

                    if (epoch >= _settings.FisoStartEpoch - 1 && epoch <= _settings.FisoEndEpoch)
                    {

                        TxOutput? input = await _dbContext.TxOutputs
                            .Include(txOut => txOut.Transaction)
                            .ThenInclude(transaction => transaction.Block)
                            .Where(txOut => txOut.TxHash == txInput.TxHash && txOut.Index == txInput.Index)
                            .FirstOrDefaultAsync();

                        if (input is null)
                        {
                            GetTransactionRequest req = new()
                            {
                                TxHashes = new List<string>() { txInput.Context.TxHash }
                            };
                            var txReq = await _transactionClient.GetTransactionUtxos(req);
                            var txOut = txReq?.Content?[0].Outputs?.Where(i => i.TxIndex == txInput.Index).FirstOrDefault();

                            if (txOut is null || txOut.PaymentAddress is null || txOut.PaymentAddress.Bech32 is null || txOut.Value is null) return;

                            input = new TxOutput()
                            {
                                Address = txOut.PaymentAddress.Bech32,
                                Amount = ulong.Parse(txOut.Value)
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
                    ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txOutput.Context.Slot);
                    ulong amount = (ulong)txOutput.Amount;

                    if (epoch >= _settings.FisoStartEpoch - 1 && epoch <= _settings.FisoEndEpoch && stakeAddress is not null)
                    {
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

                        ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txInput.Context.Slot);

                        if (input is null)
                        {
                            GetTransactionRequest req = new()
                            {
                                TxHashes = new List<string>() { txInput.Context.TxHash }
                            };
                            var txReq = await _transactionClient.GetTransactionUtxos(req);
                            var txOut = txReq?.Content?[0].Outputs?.Where(i => i.TxIndex == txInput.Index).FirstOrDefault();

                            if (txOut is null || txOut.PaymentAddress is null || txOut.PaymentAddress.Bech32 is null || txOut.Value is null) return;

                            input = new TxOutput()
                            {
                                Address = txOut.PaymentAddress.Bech32,
                                Amount = ulong.Parse(txOut.Value)
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
                    ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txOutput.Context.Slot);
                    ulong amount = txOutput.Amount;

                    if (epoch >= _settings.FisoStartEpoch - 1 && epoch <= _settings.FisoEndEpoch && stakeAddress is not null)
                    {
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
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }),
            _ => Task.Run(() => { })
        });
    }


    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;
}
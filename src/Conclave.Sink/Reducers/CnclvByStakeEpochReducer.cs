using CardanoSharp.Wallet.Models.Addresses;
using Conclave.Common.Models.Entities;
using Conclave.Sink.Data;
using Conclave.Sink.Models;
using Conclave.Sink.Models.Oura;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput, OuraVariant.TxOutput)]
public class CnclvByStakeEpochReducer : OuraReducerBase
{
    private readonly ILogger<CnclvByStakeEpochReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly ConclaveSinkSettings _settings;
    private readonly CardanoService _cardanoService;

    public CnclvByStakeEpochReducer(
        ILogger<CnclvByStakeEpochReducer> logger,
        IDbContextFactory<ConclaveSinkDbContext> dbContextFactory,
        IOptions<ConclaveSinkSettings> settings,
        CardanoService cardanoService)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _settings = settings.Value;
        _cardanoService = cardanoService;
    }

    public async Task ReduceAsync(OuraEvent ouraEvent)
    {
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        await (ouraEvent.Variant switch
        {
            OuraVariant.TxInput => Task.Run(async () =>
            {
                OuraTxInputEvent? txInputEvent = ouraEvent as OuraTxInputEvent;
                if (txInputEvent is not null &&
                    txInputEvent.TxInput is not null &&
                    txInputEvent.Context is not null &&
                    txInputEvent.Context.Slot is not null)
                {
                    TxOutput? input = await _dbContext.TxOutputs
                        .Include(txOut => txOut.Assets)
                        .Where(txOut => txOut.Assets != null && txOut.Assets.Any(asset => asset.PolicyId == _settings.ConclaveTokenPolicy && asset.Name == _settings.ConclaveTokenAssetName))
                        .Where(txOut => txOut.TxHash == txInputEvent.TxInput.TxHash && txOut.Index == txInputEvent.TxInput.Index).FirstOrDefaultAsync();

                    if (input is not null && input.Assets is not null && input.Assets.Any())
                    {
                        Asset? conclaveOutputAsset = input.Assets
                            .Where(asset => asset.PolicyId == _settings.ConclaveTokenPolicy && asset.Name == _settings.ConclaveTokenAssetName)
                            .FirstOrDefault();

                        if (conclaveOutputAsset is not null)
                        {
                            string? stakeAddress = _cardanoService.TryGetStakeAddress(input.Address);
                            if (stakeAddress is not null)
                            {
                                ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txInputEvent.Context.Slot);
                                CnclvByStakeEpoch? entry = await _dbContext.CnclvByStakeEpoch
                                    .Where((cbs) => cbs.StakeAddress == stakeAddress && cbs.Epoch == epoch)
                                    .FirstOrDefaultAsync();

                                if (entry is not null)
                                {
                                    entry.Balance -= conclaveOutputAsset.Amount;
                                }
                                else
                                {
                                    ulong prevEpochBalance = await _dbContext.CnclvByStakeEpoch
                                        .Where(w => w.StakeAddress == stakeAddress && w.Epoch < epoch)
                                        .OrderByDescending(w => w.Epoch)
                                        .Select(w => w.Balance)
                                        .FirstOrDefaultAsync();

                                    ulong total = prevEpochBalance - conclaveOutputAsset.Amount;

                                    await _dbContext.CnclvByStakeEpoch.AddAsync(new()
                                    {
                                        StakeAddress = stakeAddress,
                                        Balance = total,
                                        Epoch = epoch
                                    });
                                }
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
            }),
            OuraVariant.TxOutput => Task.Run(async () =>
            {
                OuraTxOutputEvent? txOutputEvent = ouraEvent as OuraTxOutputEvent;
                if (txOutputEvent is not null &&
                    txOutputEvent.TxOutput is not null &&
                    txOutputEvent.TxOutput.Assets is not null && txOutputEvent.TxOutput.Assets.Count() > 0 &&
                    txOutputEvent.TxOutput.Address is not null &&
                    txOutputEvent.Context is not null &&
                    txOutputEvent.Context.Slot is not null)
                {
                    string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutputEvent.TxOutput.Address);
                    if (stakeAddress is not null)
                    {
                        OuraAsset? conclaveOutputAsset = txOutputEvent.TxOutput.Assets
                            .Where(asset => asset.Policy == _settings.ConclaveTokenPolicy && asset.Asset == _settings.ConclaveTokenAssetName)
                            .FirstOrDefault();

                        if (conclaveOutputAsset is not null)
                        {
                            ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)txOutputEvent.Context.Slot);

                            CnclvByStakeEpoch? entry = await _dbContext.CnclvByStakeEpoch
                                .Where(s => s.StakeAddress == stakeAddress && s.Epoch == epoch)
                                .FirstOrDefaultAsync();

                            if (entry is not null)
                            {
                                entry.Balance += conclaveOutputAsset.Amount ?? 0;
                            }
                            else
                            {
                                ulong prevEpochBalance = await _dbContext.CnclvByStakeEpoch
                                    .Where(w => w.StakeAddress == stakeAddress && w.Epoch < epoch)
                                    .OrderByDescending(w => w.Epoch)
                                    .Select(w => w.Balance)
                                    .FirstOrDefaultAsync();

                                await _dbContext.CnclvByStakeEpoch.AddAsync(new()
                                {
                                    StakeAddress = stakeAddress,
                                    Balance = prevEpochBalance + conclaveOutputAsset.Amount ?? 0,
                                    Epoch = epoch
                                });
                            }
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                }
            }),
            _ => Task.CompletedTask
        });
    }

    public async Task RollbackAsync(Block rollbackBlock)
    {
        ulong epoch = _cardanoService.CalculateEpochBySlot((ulong)rollbackBlock.Slot);
        using ConclaveSinkDbContext _dbContext = await _dbContextFactory.CreateDbContextAsync();
        IEnumerable<TxInput> consumed = await _dbContext.TxInputs
            .Include(txInput => txInput.TxOutput)
            .ThenInclude(txOutput => txOutput.Assets)
            .Include(txInput => txInput.Transaction)
            .ThenInclude(tx => tx.Block)
            .Where(txInput => txInput.Transaction.Block == rollbackBlock)
            .Where(txInput => txInput.TxOutput.Assets != null && txInput.TxOutput.Assets.Any(a => a.PolicyId == _settings.ConclaveTokenPolicy && a.Name == _settings.ConclaveTokenAssetName))
            .ToListAsync();
        IEnumerable<TxOutput> produced = await _dbContext.TxOutputs
            .Include(txOutput => txOutput.Assets)
            .Include(txOutput => txOutput.Transaction)
            .ThenInclude(tx => tx.Block)
            .Where(txOutput => txOutput.Transaction.Block == rollbackBlock)
            .Where(txOutput => txOutput.Assets != null && txOutput.Assets.Any(a => a.PolicyId == _settings.ConclaveTokenPolicy && a.Name == _settings.ConclaveTokenAssetName))
            .ToListAsync();

        // process consumed
        IEnumerable<Task> consumeTasks = consumed.ToList().Select(txInput => Task.Run(async () =>
        {
            if (txInput.TxOutput is not null)
            {
                string? stakeAddress = _cardanoService.TryGetStakeAddress(txInput.TxOutput.Address);

                CnclvByStakeEpoch? entry = await _dbContext.CnclvByStakeEpoch
                    .Where((s) => s.StakeAddress == stakeAddress && s.Epoch == epoch)
                    .FirstOrDefaultAsync();

                if (entry is not null && txInput.TxOutput.Assets is not null)
                {
                    Asset? conclaveOutputAsset = txInput.TxOutput.Assets
                            .Where(asset => asset.PolicyId == _settings.ConclaveTokenPolicy && asset.Name == _settings.ConclaveTokenAssetName)
                            .FirstOrDefault();

                    if (conclaveOutputAsset is not null)
                        entry.Balance += conclaveOutputAsset.Amount;
                }
            }
        }));

        foreach (Task consumeTask in consumeTasks) await consumeTask;

        // process produced
        IEnumerable<Task> produceTasks = produced.ToList().Select(txOutput => Task.Run(async () =>
        {
            string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutput.Address);
            CnclvByStakeEpoch? entry = await _dbContext.CnclvByStakeEpoch
                .Where((s) => s.StakeAddress == stakeAddress && s.Epoch == epoch)
                .FirstOrDefaultAsync();

            if (entry is not null && txOutput.Assets is not null)
            {
                ulong prevEpochBalance = await _dbContext.CnclvByStakeEpoch
                    .Where(w => w.StakeAddress == stakeAddress && w.Epoch < epoch)
                    .OrderByDescending(w => w.Epoch)
                    .Select(w => w.Balance)
                    .FirstOrDefaultAsync();

                Asset? conclaveOutputAsset = txOutput.Assets
                        .Where(asset => asset.PolicyId == _settings.ConclaveTokenPolicy && asset.Name == _settings.ConclaveTokenAssetName)
                        .FirstOrDefault();

                if (conclaveOutputAsset is not null)
                {
                    entry.Balance -= conclaveOutputAsset.Amount;

                    if (entry.Balance <= 0 || entry.Balance <= prevEpochBalance)
                    {
                        _dbContext.CnclvByStakeEpoch.Remove(entry);
                    }
                }
            }
        }));

        foreach (Task produceTask in produceTasks) await produceTask;

        await _dbContext.SaveChangesAsync();
    }
}
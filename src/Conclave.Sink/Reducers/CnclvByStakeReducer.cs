using Conclave.Sink.Data;
using Conclave.Common.Models;
using Conclave.Sink.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Conclave.Sink.Models.Oura;
using Conclave.Sink.Models;

namespace Conclave.Sink.Reducers;

[OuraReducer(OuraVariant.TxInput, OuraVariant.TxOutput)]
public class CnclvByStakeReducer : OuraReducerBase
{
    private readonly ILogger<CnclvByStakeReducer> _logger;
    private IDbContextFactory<ConclaveSinkDbContext> _dbContextFactory;
    private readonly ConclaveSinkSettings _settings;
    private readonly CardanoService _cardanoService;

    public CnclvByStakeReducer(
        ILogger<CnclvByStakeReducer> logger,
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
                if (txInputEvent is not null && txInputEvent.TxInput is not null)
                {
                    TxOutput? input = await _dbContext.TxOutputs
                        .Include(txOut => txOut.Assets)
                        .Where(txOut => txOut.TxHash == txInputEvent.TxInput.TxHash && txOut.Index == txInputEvent.TxInput.Index).FirstOrDefaultAsync();

                    if (input is not null && input.Assets is not null)
                    {
                        Asset? conclaveOutputAsset = input.Assets
                            .Where(asset => asset.PolicyId == _settings.ConclaveTokenPolicy && asset.Name == _settings.ConclaveTokenAssetName)
                            .FirstOrDefault();

                        if (conclaveOutputAsset is not null)
                        {
                            string? stakeAddress = _cardanoService.TryGetStakeAddress(input.Address);
                            if (stakeAddress is not null)
                            {
                                CnclvByStakeEpoch? entry = await _dbContext.CnclvByStakeEpoch
                                    .Where((cbs) => cbs.StakeAddress == stakeAddress)
                                    .FirstOrDefaultAsync();

                                if (entry is not null)
                                {
                                    entry.Balance -= conclaveOutputAsset.Amount;
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
                    txOutputEvent.TxOutput.Address is not null)
                {
                    string? stakeAddress = _cardanoService.TryGetStakeAddress(txOutputEvent.TxOutput.Address);

                    if (stakeAddress is not null)
                    {
                        OuraAsset? conclaveOutputAsset = txOutputEvent.TxOutput.Assets
                            .Where(asset => asset.Policy == _settings.ConclaveTokenPolicy && asset.Asset == _settings.ConclaveTokenAssetName)
                            .FirstOrDefault();

                        if (conclaveOutputAsset is not null)
                        {
                            CnclvByStakeEpoch? entry = await _dbContext.CnclvByStakeEpoch.Where(s => s.StakeAddress == stakeAddress).FirstOrDefaultAsync();
                            if (entry is not null)
                            {
                                entry.Balance += conclaveOutputAsset.Amount ?? 0;
                            }
                            else
                            {
                                await _dbContext.CnclvByStakeEpoch.AddAsync(new()
                                {
                                    StakeAddress = stakeAddress,
                                    Balance = conclaveOutputAsset.Amount ?? 0
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

    public async Task RollbackAsync(Block rollbackBlock) => await Task.CompletedTask;

}
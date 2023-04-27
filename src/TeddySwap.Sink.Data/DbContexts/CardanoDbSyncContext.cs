using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TeddySwap.Common.Models.CardanoDbSync;

namespace TeddySwap.Sink.Data;

public partial class CardanoDbSyncContext : DbContext
{
    public CardanoDbSyncContext()
    {
    }

    public CardanoDbSyncContext(DbContextOptions<CardanoDbSyncContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdaPot> AdaPots { get; set; }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<CollateralTxIn> CollateralTxIns { get; set; }

    public virtual DbSet<CollateralTxOut> CollateralTxOuts { get; set; }

    public virtual DbSet<CostModel> CostModels { get; set; }

    public virtual DbSet<Datum> Data { get; set; }

    public virtual DbSet<Delegation> Delegations { get; set; }

    public virtual DbSet<DelistedPool> DelistedPools { get; set; }

    public virtual DbSet<Epoch> Epoches { get; set; }

    public virtual DbSet<EpochParam> EpochParams { get; set; }

    public virtual DbSet<EpochStake> EpochStakes { get; set; }

    public virtual DbSet<EpochSyncTime> EpochSyncTimes { get; set; }

    public virtual DbSet<ExtraKeyWitness> ExtraKeyWitnesses { get; set; }

    public virtual DbSet<IndexBloat> IndexBloats { get; set; }

    public virtual DbSet<MaTxMint> MaTxMints { get; set; }

    public virtual DbSet<MaTxOut> MaTxOuts { get; set; }

    public virtual DbSet<Metum> Meta { get; set; }

    public virtual DbSet<MultiAsset> MultiAssets { get; set; }

    public virtual DbSet<ParamProposal> ParamProposals { get; set; }

    public virtual DbSet<PgStatStatement> PgStatStatements { get; set; }

    public virtual DbSet<PoolHash> PoolHashes { get; set; }

    public virtual DbSet<PoolMetadataRef> PoolMetadataRefs { get; set; }

    public virtual DbSet<PoolOfflineDatum> PoolOfflineData { get; set; }

    public virtual DbSet<PoolOfflineFetchError> PoolOfflineFetchErrors { get; set; }

    public virtual DbSet<PoolOwner> PoolOwners { get; set; }

    public virtual DbSet<PoolRelay> PoolRelays { get; set; }

    public virtual DbSet<PoolRetire> PoolRetires { get; set; }

    public virtual DbSet<PoolUpdate> PoolUpdates { get; set; }

    public virtual DbSet<PotTransfer> PotTransfers { get; set; }

    public virtual DbSet<Redeemer> Redeemers { get; set; }

    public virtual DbSet<RedeemerDatum> RedeemerData { get; set; }

    public virtual DbSet<ReferenceTxIn> ReferenceTxIns { get; set; }

    public virtual DbSet<Reserve> Reserves { get; set; }

    public virtual DbSet<ReservedPoolTicker> ReservedPoolTickers { get; set; }

    public virtual DbSet<Reward> Rewards { get; set; }

    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }

    public virtual DbSet<Script> Scripts { get; set; }

    public virtual DbSet<SlotLeader> SlotLeaders { get; set; }

    public virtual DbSet<StakeAddress> StakeAddresses { get; set; }

    public virtual DbSet<StakeDeregistration> StakeDeregistrations { get; set; }

    public virtual DbSet<StakeRegistration> StakeRegistrations { get; set; }

    public virtual DbSet<TableBloat> TableBloats { get; set; }

    public virtual DbSet<Treasury> Treasuries { get; set; }

    public virtual DbSet<Tx> Txes { get; set; }

    public virtual DbSet<TxIn> TxIns { get; set; }

    public virtual DbSet<TxMetadatum> TxMetadata { get; set; }

    public virtual DbSet<TxOut> TxOuts { get; set; }

    public virtual DbSet<UtxoByronView> UtxoByronViews { get; set; }

    public virtual DbSet<UtxoView> UtxoViews { get; set; }

    public virtual DbSet<Withdrawal> Withdrawals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("rewardtype", new[] { "leader", "member", "reserves", "treasury", "refund" })
            .HasPostgresEnum("scriptpurposetype", new[] { "spend", "mint", "cert", "reward" })
            .HasPostgresEnum("scripttype", new[] { "multisig", "timelock", "plutusV1", "plutusV2" })
            .HasPostgresEnum("syncstatetype", new[] { "lagging", "following" })
            .HasPostgresExtension("pg_stat_kcache")
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("set_user");

        modelBuilder.Entity<AdaPot>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ada_pots_pkey");

            entity.ToTable("ada_pots");

            entity.HasIndex(e => e.BlockId, "unique_ada_pots").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.Deposits)
                .HasPrecision(20)
                .HasColumnName("deposits");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.Fees)
                .HasPrecision(20)
                .HasColumnName("fees");
            entity.Property(e => e.Reserves)
                .HasPrecision(20)
                .HasColumnName("reserves");
            entity.Property(e => e.Rewards)
                .HasPrecision(20)
                .HasColumnName("rewards");
            entity.Property(e => e.SlotNo).HasColumnName("slot_no");
            entity.Property(e => e.Treasury)
                .HasPrecision(20)
                .HasColumnName("treasury");
            entity.Property(e => e.Utxo)
                .HasPrecision(20)
                .HasColumnName("utxo");

            entity.HasOne(d => d.Block).WithOne(p => p.AdaPot)
                .HasForeignKey<AdaPot>(d => d.BlockId)
                .HasConstraintName("ada_pots_block_id_fkey");
        });

        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("block_pkey");

            entity.ToTable("block");

            entity.HasIndex(e => e.BlockNo, "idx_block_block_no");

            entity.HasIndex(e => e.EpochNo, "idx_block_epoch_no");

            entity.HasIndex(e => e.PreviousId, "idx_block_previous_id");

            entity.HasIndex(e => e.SlotLeaderId, "idx_block_slot_leader_id");

            entity.HasIndex(e => e.SlotNo, "idx_block_slot_no");

            entity.HasIndex(e => e.Time, "idx_block_time");

            entity.HasIndex(e => e.Hash, "unique_block").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockNo).HasColumnName("block_no");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.EpochSlotNo).HasColumnName("epoch_slot_no");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.OpCert).HasColumnName("op_cert");
            entity.Property(e => e.OpCertCounter).HasColumnName("op_cert_counter");
            entity.Property(e => e.PreviousId).HasColumnName("previous_id");
            entity.Property(e => e.ProtoMajor).HasColumnName("proto_major");
            entity.Property(e => e.ProtoMinor).HasColumnName("proto_minor");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.SlotLeaderId).HasColumnName("slot_leader_id");
            entity.Property(e => e.SlotNo).HasColumnName("slot_no");
            entity.Property(e => e.Time)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("time");
            entity.Property(e => e.TxCount).HasColumnName("tx_count");
            entity.Property(e => e.VrfKey)
                .HasColumnType("character varying")
                .HasColumnName("vrf_key");

            entity.HasOne(d => d.Previous).WithMany(p => p.InversePrevious)
                .HasForeignKey(d => d.PreviousId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("block_previous_id_fkey");

            entity.HasOne(d => d.SlotLeader).WithMany(p => p.Blocks)
                .HasForeignKey(d => d.SlotLeaderId)
                .HasConstraintName("block_slot_leader_id_fkey");
        });

        modelBuilder.Entity<CollateralTxIn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("collateral_tx_in_pkey");

            entity.ToTable("collateral_tx_in");

            entity.HasIndex(e => e.TxOutId, "idx_collateral_tx_in_tx_out_id");

            entity.HasIndex(e => new { e.TxInId, e.TxOutId, e.TxOutIndex }, "unique_col_txin").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TxInId).HasColumnName("tx_in_id");
            entity.Property(e => e.TxOutId).HasColumnName("tx_out_id");
            entity.Property(e => e.TxOutIndex).HasColumnName("tx_out_index");

            entity.HasOne(d => d.TxIn).WithMany(p => p.CollateralTxInTxIns)
                .HasForeignKey(d => d.TxInId)
                .HasConstraintName("collateral_tx_in_tx_in_id_fkey");

            entity.HasOne(d => d.TxOut).WithMany(p => p.CollateralTxInTxOuts)
                .HasForeignKey(d => d.TxOutId)
                .HasConstraintName("collateral_tx_in_tx_out_id_fkey");
        });

        modelBuilder.Entity<CollateralTxOut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("collateral_tx_out_pkey");

            entity.ToTable("collateral_tx_out");

            entity.HasIndex(e => e.InlineDatumId, "collateral_tx_out_inline_datum_id_idx");

            entity.HasIndex(e => e.ReferenceScriptId, "collateral_tx_out_reference_script_id_idx");

            entity.HasIndex(e => e.StakeAddressId, "collateral_tx_out_stake_address_id_idx");

            entity.HasIndex(e => new { e.TxId, e.Index }, "unique_col_txout").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.AddressHasScript).HasColumnName("address_has_script");
            entity.Property(e => e.AddressRaw).HasColumnName("address_raw");
            entity.Property(e => e.DataHash).HasColumnName("data_hash");
            entity.Property(e => e.Index).HasColumnName("index");
            entity.Property(e => e.InlineDatumId).HasColumnName("inline_datum_id");
            entity.Property(e => e.MultiAssetsDescr)
                .HasColumnType("character varying")
                .HasColumnName("multi_assets_descr");
            entity.Property(e => e.PaymentCred).HasColumnName("payment_cred");
            entity.Property(e => e.ReferenceScriptId).HasColumnName("reference_script_id");
            entity.Property(e => e.StakeAddressId).HasColumnName("stake_address_id");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.Value)
                .HasPrecision(20)
                .HasColumnName("value");

            entity.HasOne(d => d.InlineDatum).WithMany(p => p.CollateralTxOuts)
                .HasForeignKey(d => d.InlineDatumId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("collateral_tx_out_inline_datum_id_fkey");

            entity.HasOne(d => d.ReferenceScript).WithMany(p => p.CollateralTxOuts)
                .HasForeignKey(d => d.ReferenceScriptId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("collateral_tx_out_reference_script_id_fkey");

            entity.HasOne(d => d.StakeAddress).WithMany(p => p.CollateralTxOuts)
                .HasForeignKey(d => d.StakeAddressId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("collateral_tx_out_stake_address_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.CollateralTxOuts)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("collateral_tx_out_tx_id_fkey");
        });

        modelBuilder.Entity<CostModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cost_model_pkey");

            entity.ToTable("cost_model");

            entity.HasIndex(e => e.BlockId, "idx_cost_model_block_id");

            entity.HasIndex(e => e.Hash, "unique_cost_model").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.Costs)
                .HasColumnType("jsonb")
                .HasColumnName("costs");
            entity.Property(e => e.Hash).HasColumnName("hash");

            entity.HasOne(d => d.Block).WithMany(p => p.CostModels)
                .HasForeignKey(d => d.BlockId)
                .HasConstraintName("cost_model_block_id_fkey");
        });

        modelBuilder.Entity<Datum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("datum_pkey");

            entity.ToTable("datum");

            entity.HasIndex(e => e.TxId, "idx_datum_tx_id");

            entity.HasIndex(e => e.Hash, "unique_datum").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bytes).HasColumnName("bytes");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.Value)
                .HasColumnType("jsonb")
                .HasColumnName("value");

            entity.HasOne(d => d.Tx).WithMany(p => p.Data)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("datum_tx_id_fkey");
        });

        modelBuilder.Entity<Delegation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("delegation_pkey");

            entity.ToTable("delegation");

            entity.HasIndex(e => e.ActiveEpochNo, "idx_delegation_active_epoch_no");

            entity.HasIndex(e => e.AddrId, "idx_delegation_addr_id");

            entity.HasIndex(e => e.PoolHashId, "idx_delegation_pool_hash_id");

            entity.HasIndex(e => e.RedeemerId, "idx_delegation_redeemer_id");

            entity.HasIndex(e => e.TxId, "idx_delegation_tx_id");

            entity.HasIndex(e => new { e.TxId, e.CertIndex }, "unique_delegation").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActiveEpochNo).HasColumnName("active_epoch_no");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.PoolHashId).HasColumnName("pool_hash_id");
            entity.Property(e => e.RedeemerId).HasColumnName("redeemer_id");
            entity.Property(e => e.SlotNo).HasColumnName("slot_no");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.Delegations)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("delegation_addr_id_fkey");

            entity.HasOne(d => d.PoolHash).WithMany(p => p.Delegations)
                .HasForeignKey(d => d.PoolHashId)
                .HasConstraintName("delegation_pool_hash_id_fkey");

            entity.HasOne(d => d.Redeemer).WithMany(p => p.Delegations)
                .HasForeignKey(d => d.RedeemerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("delegation_redeemer_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.Delegations)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("delegation_tx_id_fkey");
        });

        modelBuilder.Entity<DelistedPool>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("delisted_pool_pkey");

            entity.ToTable("delisted_pool");

            entity.HasIndex(e => e.HashRaw, "unique_delisted_pool").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HashRaw).HasColumnName("hash_raw");
        });

        modelBuilder.Entity<Epoch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epoch_pkey");

            entity.ToTable("epoch");

            entity.HasIndex(e => e.No, "idx_epoch_no");

            entity.HasIndex(e => e.No, "unique_epoch").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlkCount).HasColumnName("blk_count");
            entity.Property(e => e.EndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_time");
            entity.Property(e => e.Fees)
                .HasPrecision(20)
                .HasColumnName("fees");
            entity.Property(e => e.No).HasColumnName("no");
            entity.Property(e => e.OutSum)
                .HasPrecision(39)
                .HasColumnName("out_sum");
            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_time");
            entity.Property(e => e.TxCount).HasColumnName("tx_count");
        });

        modelBuilder.Entity<EpochParam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epoch_param_pkey");

            entity.ToTable("epoch_param");

            entity.HasIndex(e => e.BlockId, "idx_epoch_param_block_id");

            entity.HasIndex(e => e.CostModelId, "idx_epoch_param_cost_model_id");

            entity.HasIndex(e => new { e.EpochNo, e.BlockId }, "unique_epoch_param").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.CoinsPerUtxoSize)
                .HasPrecision(20)
                .HasColumnName("coins_per_utxo_size");
            entity.Property(e => e.CollateralPercent).HasColumnName("collateral_percent");
            entity.Property(e => e.CostModelId).HasColumnName("cost_model_id");
            entity.Property(e => e.Decentralisation).HasColumnName("decentralisation");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.ExtraEntropy).HasColumnName("extra_entropy");
            entity.Property(e => e.Influence).HasColumnName("influence");
            entity.Property(e => e.KeyDeposit)
                .HasPrecision(20)
                .HasColumnName("key_deposit");
            entity.Property(e => e.MaxBhSize).HasColumnName("max_bh_size");
            entity.Property(e => e.MaxBlockExMem)
                .HasPrecision(20)
                .HasColumnName("max_block_ex_mem");
            entity.Property(e => e.MaxBlockExSteps)
                .HasPrecision(20)
                .HasColumnName("max_block_ex_steps");
            entity.Property(e => e.MaxBlockSize).HasColumnName("max_block_size");
            entity.Property(e => e.MaxCollateralInputs).HasColumnName("max_collateral_inputs");
            entity.Property(e => e.MaxEpoch).HasColumnName("max_epoch");
            entity.Property(e => e.MaxTxExMem)
                .HasPrecision(20)
                .HasColumnName("max_tx_ex_mem");
            entity.Property(e => e.MaxTxExSteps)
                .HasPrecision(20)
                .HasColumnName("max_tx_ex_steps");
            entity.Property(e => e.MaxTxSize).HasColumnName("max_tx_size");
            entity.Property(e => e.MaxValSize)
                .HasPrecision(20)
                .HasColumnName("max_val_size");
            entity.Property(e => e.MinFeeA).HasColumnName("min_fee_a");
            entity.Property(e => e.MinFeeB).HasColumnName("min_fee_b");
            entity.Property(e => e.MinPoolCost)
                .HasPrecision(20)
                .HasColumnName("min_pool_cost");
            entity.Property(e => e.MinUtxoValue)
                .HasPrecision(20)
                .HasColumnName("min_utxo_value");
            entity.Property(e => e.MonetaryExpandRate).HasColumnName("monetary_expand_rate");
            entity.Property(e => e.Nonce).HasColumnName("nonce");
            entity.Property(e => e.OptimalPoolCount).HasColumnName("optimal_pool_count");
            entity.Property(e => e.PoolDeposit)
                .HasPrecision(20)
                .HasColumnName("pool_deposit");
            entity.Property(e => e.PriceMem).HasColumnName("price_mem");
            entity.Property(e => e.PriceStep).HasColumnName("price_step");
            entity.Property(e => e.ProtocolMajor).HasColumnName("protocol_major");
            entity.Property(e => e.ProtocolMinor).HasColumnName("protocol_minor");
            entity.Property(e => e.TreasuryGrowthRate).HasColumnName("treasury_growth_rate");

            entity.HasOne(d => d.Block).WithMany(p => p.EpochParams)
                .HasForeignKey(d => d.BlockId)
                .HasConstraintName("epoch_param_block_id_fkey");

            entity.HasOne(d => d.CostModel).WithMany(p => p.EpochParams)
                .HasForeignKey(d => d.CostModelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("epoch_param_cost_model_id_fkey");
        });

        modelBuilder.Entity<EpochStake>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epoch_stake_pkey");

            entity.ToTable("epoch_stake");

            entity.HasIndex(e => e.AddrId, "idx_epoch_stake_addr_id");

            entity.HasIndex(e => e.EpochNo, "idx_epoch_stake_epoch_no");

            entity.HasIndex(e => e.PoolId, "idx_epoch_stake_pool_id");

            entity.HasIndex(e => new { e.EpochNo, e.AddrId, e.PoolId }, "unique_stake").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.Amount)
                .HasPrecision(20)
                .HasColumnName("amount");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.PoolId).HasColumnName("pool_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.EpochStakes)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("epoch_stake_addr_id_fkey");

            entity.HasOne(d => d.Pool).WithMany(p => p.EpochStakes)
                .HasForeignKey(d => d.PoolId)
                .HasConstraintName("epoch_stake_pool_id_fkey");
        });

        modelBuilder.Entity<EpochSyncTime>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("epoch_sync_time_pkey");

            entity.ToTable("epoch_sync_time");

            entity.HasIndex(e => e.No, "unique_epoch_sync_time").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.No).HasColumnName("no");
            entity.Property(e => e.Seconds).HasColumnName("seconds");
        });

        modelBuilder.Entity<ExtraKeyWitness>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("extra_key_witness_pkey");

            entity.ToTable("extra_key_witness");

            entity.HasIndex(e => e.TxId, "idx_extra_key_witness_tx_id");

            entity.HasIndex(e => e.Hash, "unique_witness").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Tx).WithMany(p => p.ExtraKeyWitnesses)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("extra_key_witness_tx_id_fkey");
        });

        modelBuilder.Entity<IndexBloat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("index_bloat", "metric_helpers");

            entity.Property(e => e.IBloatRatio).HasColumnName("i_bloat_ratio");
            entity.Property(e => e.IBloatSize).HasColumnName("i_bloat_size");
            entity.Property(e => e.IExtraRatio).HasColumnName("i_extra_ratio");
            entity.Property(e => e.IExtraSize).HasColumnName("i_extra_size");
            entity.Property(e => e.IFillFactor).HasColumnName("i_fill_factor");
            entity.Property(e => e.IIsNa).HasColumnName("i_is_na");
            entity.Property(e => e.IRealSize).HasColumnName("i_real_size");
        });

        modelBuilder.Entity<MaTxMint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ma_tx_mint_pkey");

            entity.ToTable("ma_tx_mint");

            entity.HasIndex(e => e.TxId, "idx_ma_tx_mint_tx_id");

            entity.HasIndex(e => new { e.Ident, e.TxId }, "unique_ma_tx_mint").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ident).HasColumnName("ident");
            entity.Property(e => e.Quantity)
                .HasPrecision(20)
                .HasColumnName("quantity");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.IdentNavigation).WithMany(p => p.MaTxMints)
                .HasForeignKey(d => d.Ident)
                .HasConstraintName("ma_tx_mint_ident_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.MaTxMints)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("ma_tx_mint_tx_id_fkey");
        });

        modelBuilder.Entity<MaTxOut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("ma_tx_out_pkey");

            entity.ToTable("ma_tx_out");

            entity.HasIndex(e => e.TxOutId, "idx_ma_tx_out_tx_out_id");

            entity.HasIndex(e => new { e.Ident, e.TxOutId }, "unique_ma_tx_out").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ident).HasColumnName("ident");
            entity.Property(e => e.Quantity)
                .HasPrecision(20)
                .HasColumnName("quantity");
            entity.Property(e => e.TxOutId).HasColumnName("tx_out_id");

            entity.HasOne(d => d.IdentNavigation).WithMany(p => p.MaTxOuts)
                .HasForeignKey(d => d.Ident)
                .HasConstraintName("ma_tx_out_ident_fkey");

            entity.HasOne(d => d.TxOut).WithMany(p => p.MaTxOuts)
                .HasForeignKey(d => d.TxOutId)
                .HasConstraintName("ma_tx_out_tx_out_id_fkey");
        });

        modelBuilder.Entity<Metum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("meta_pkey");

            entity.ToTable("meta");

            entity.HasIndex(e => e.StartTime, "unique_meta").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NetworkName)
                .HasColumnType("character varying")
                .HasColumnName("network_name");
            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_time");
            entity.Property(e => e.Version)
                .HasColumnType("character varying")
                .HasColumnName("version");
        });

        modelBuilder.Entity<MultiAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("multi_asset_pkey");

            entity.ToTable("multi_asset");

            entity.HasIndex(e => new { e.Policy, e.Name }, "unique_multi_asset").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fingerprint)
                .HasColumnType("character varying")
                .HasColumnName("fingerprint");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Policy).HasColumnName("policy");
        });

        modelBuilder.Entity<ParamProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("param_proposal_pkey");

            entity.ToTable("param_proposal");

            entity.HasIndex(e => e.CostModelId, "idx_param_proposal_cost_model_id");

            entity.HasIndex(e => e.RegisteredTxId, "idx_param_proposal_registered_tx_id");

            entity.HasIndex(e => new { e.Key, e.RegisteredTxId }, "unique_param_proposal").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CoinsPerUtxoSize)
                .HasPrecision(20)
                .HasColumnName("coins_per_utxo_size");
            entity.Property(e => e.CollateralPercent).HasColumnName("collateral_percent");
            entity.Property(e => e.CostModelId).HasColumnName("cost_model_id");
            entity.Property(e => e.Decentralisation).HasColumnName("decentralisation");
            entity.Property(e => e.Entropy).HasColumnName("entropy");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.Influence).HasColumnName("influence");
            entity.Property(e => e.Key).HasColumnName("key");
            entity.Property(e => e.KeyDeposit)
                .HasPrecision(20)
                .HasColumnName("key_deposit");
            entity.Property(e => e.MaxBhSize)
                .HasPrecision(20)
                .HasColumnName("max_bh_size");
            entity.Property(e => e.MaxBlockExMem)
                .HasPrecision(20)
                .HasColumnName("max_block_ex_mem");
            entity.Property(e => e.MaxBlockExSteps)
                .HasPrecision(20)
                .HasColumnName("max_block_ex_steps");
            entity.Property(e => e.MaxBlockSize)
                .HasPrecision(20)
                .HasColumnName("max_block_size");
            entity.Property(e => e.MaxCollateralInputs).HasColumnName("max_collateral_inputs");
            entity.Property(e => e.MaxEpoch)
                .HasPrecision(20)
                .HasColumnName("max_epoch");
            entity.Property(e => e.MaxTxExMem)
                .HasPrecision(20)
                .HasColumnName("max_tx_ex_mem");
            entity.Property(e => e.MaxTxExSteps)
                .HasPrecision(20)
                .HasColumnName("max_tx_ex_steps");
            entity.Property(e => e.MaxTxSize)
                .HasPrecision(20)
                .HasColumnName("max_tx_size");
            entity.Property(e => e.MaxValSize)
                .HasPrecision(20)
                .HasColumnName("max_val_size");
            entity.Property(e => e.MinFeeA)
                .HasPrecision(20)
                .HasColumnName("min_fee_a");
            entity.Property(e => e.MinFeeB)
                .HasPrecision(20)
                .HasColumnName("min_fee_b");
            entity.Property(e => e.MinPoolCost)
                .HasPrecision(20)
                .HasColumnName("min_pool_cost");
            entity.Property(e => e.MinUtxoValue)
                .HasPrecision(20)
                .HasColumnName("min_utxo_value");
            entity.Property(e => e.MonetaryExpandRate).HasColumnName("monetary_expand_rate");
            entity.Property(e => e.OptimalPoolCount)
                .HasPrecision(20)
                .HasColumnName("optimal_pool_count");
            entity.Property(e => e.PoolDeposit)
                .HasPrecision(20)
                .HasColumnName("pool_deposit");
            entity.Property(e => e.PriceMem).HasColumnName("price_mem");
            entity.Property(e => e.PriceStep).HasColumnName("price_step");
            entity.Property(e => e.ProtocolMajor).HasColumnName("protocol_major");
            entity.Property(e => e.ProtocolMinor).HasColumnName("protocol_minor");
            entity.Property(e => e.RegisteredTxId).HasColumnName("registered_tx_id");
            entity.Property(e => e.TreasuryGrowthRate).HasColumnName("treasury_growth_rate");

            entity.HasOne(d => d.CostModel).WithMany(p => p.ParamProposals)
                .HasForeignKey(d => d.CostModelId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("param_proposal_cost_model_id_fkey");

            entity.HasOne(d => d.RegisteredTx).WithMany(p => p.ParamProposals)
                .HasForeignKey(d => d.RegisteredTxId)
                .HasConstraintName("param_proposal_registered_tx_id_fkey");
        });

        modelBuilder.Entity<PgStatStatement>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("pg_stat_statements", "metric_helpers");

            entity.Property(e => e.BlkReadTime).HasColumnName("blk_read_time");
            entity.Property(e => e.BlkWriteTime).HasColumnName("blk_write_time");
            entity.Property(e => e.Calls).HasColumnName("calls");
            entity.Property(e => e.Dbid)
                .HasColumnType("oid")
                .HasColumnName("dbid");
            entity.Property(e => e.LocalBlksDirtied).HasColumnName("local_blks_dirtied");
            entity.Property(e => e.LocalBlksHit).HasColumnName("local_blks_hit");
            entity.Property(e => e.LocalBlksRead).HasColumnName("local_blks_read");
            entity.Property(e => e.LocalBlksWritten).HasColumnName("local_blks_written");
            entity.Property(e => e.MaxExecTime).HasColumnName("max_exec_time");
            entity.Property(e => e.MaxPlanTime).HasColumnName("max_plan_time");
            entity.Property(e => e.MeanExecTime).HasColumnName("mean_exec_time");
            entity.Property(e => e.MeanPlanTime).HasColumnName("mean_plan_time");
            entity.Property(e => e.MinExecTime).HasColumnName("min_exec_time");
            entity.Property(e => e.MinPlanTime).HasColumnName("min_plan_time");
            entity.Property(e => e.Plans).HasColumnName("plans");
            entity.Property(e => e.Query).HasColumnName("query");
            entity.Property(e => e.Queryid).HasColumnName("queryid");
            entity.Property(e => e.Rows).HasColumnName("rows");
            entity.Property(e => e.SharedBlksDirtied).HasColumnName("shared_blks_dirtied");
            entity.Property(e => e.SharedBlksHit).HasColumnName("shared_blks_hit");
            entity.Property(e => e.SharedBlksRead).HasColumnName("shared_blks_read");
            entity.Property(e => e.SharedBlksWritten).HasColumnName("shared_blks_written");
            entity.Property(e => e.StddevExecTime).HasColumnName("stddev_exec_time");
            entity.Property(e => e.StddevPlanTime).HasColumnName("stddev_plan_time");
            entity.Property(e => e.TempBlksRead).HasColumnName("temp_blks_read");
            entity.Property(e => e.TempBlksWritten).HasColumnName("temp_blks_written");
            entity.Property(e => e.Toplevel).HasColumnName("toplevel");
            entity.Property(e => e.TotalExecTime).HasColumnName("total_exec_time");
            entity.Property(e => e.TotalPlanTime).HasColumnName("total_plan_time");
            entity.Property(e => e.Userid)
                .HasColumnType("oid")
                .HasColumnName("userid");
            entity.Property(e => e.WalBytes).HasColumnName("wal_bytes");
            entity.Property(e => e.WalFpi).HasColumnName("wal_fpi");
            entity.Property(e => e.WalRecords).HasColumnName("wal_records");
        });

        modelBuilder.Entity<PoolHash>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_hash_pkey");

            entity.ToTable("pool_hash");

            entity.HasIndex(e => e.HashRaw, "unique_pool_hash").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HashRaw).HasColumnName("hash_raw");
            entity.Property(e => e.View)
                .HasColumnType("character varying")
                .HasColumnName("view");
        });

        modelBuilder.Entity<PoolMetadataRef>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_metadata_ref_pkey");

            entity.ToTable("pool_metadata_ref");

            entity.HasIndex(e => e.RegisteredTxId, "idx_pool_metadata_ref_registered_tx_id");

            entity.HasIndex(e => new { e.PoolId, e.Url, e.Hash }, "unique_pool_metadata_ref").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.PoolId).HasColumnName("pool_id");
            entity.Property(e => e.RegisteredTxId).HasColumnName("registered_tx_id");
            entity.Property(e => e.Url)
                .HasColumnType("character varying")
                .HasColumnName("url");

            entity.HasOne(d => d.Pool).WithMany(p => p.PoolMetadataRefs)
                .HasForeignKey(d => d.PoolId)
                .HasConstraintName("pool_metadata_ref_pool_id_fkey");

            entity.HasOne(d => d.RegisteredTx).WithMany(p => p.PoolMetadataRefs)
                .HasForeignKey(d => d.RegisteredTxId)
                .HasConstraintName("pool_metadata_ref_registered_tx_id_fkey");
        });

        modelBuilder.Entity<PoolOfflineDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_offline_data_pkey");

            entity.ToTable("pool_offline_data");

            entity.HasIndex(e => e.PmrId, "idx_pool_offline_data_pmr_id");

            entity.HasIndex(e => new { e.PoolId, e.Hash }, "unique_pool_offline_data").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bytes).HasColumnName("bytes");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.Json)
                .HasColumnType("jsonb")
                .HasColumnName("json");
            entity.Property(e => e.PmrId).HasColumnName("pmr_id");
            entity.Property(e => e.PoolId).HasColumnName("pool_id");
            entity.Property(e => e.TickerName)
                .HasColumnType("character varying")
                .HasColumnName("ticker_name");

            entity.HasOne(d => d.Pmr).WithMany(p => p.PoolOfflineData)
                .HasForeignKey(d => d.PmrId)
                .HasConstraintName("pool_offline_data_pmr_id_fkey");

            entity.HasOne(d => d.Pool).WithMany(p => p.PoolOfflineData)
                .HasForeignKey(d => d.PoolId)
                .HasConstraintName("pool_offline_data_pool_id_fkey");
        });

        modelBuilder.Entity<PoolOfflineFetchError>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_offline_fetch_error_pkey");

            entity.ToTable("pool_offline_fetch_error");

            entity.HasIndex(e => e.PmrId, "idx_pool_offline_fetch_error_pmr_id");

            entity.HasIndex(e => new { e.PoolId, e.FetchTime, e.RetryCount }, "unique_pool_offline_fetch_error").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FetchError)
                .HasColumnType("character varying")
                .HasColumnName("fetch_error");
            entity.Property(e => e.FetchTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("fetch_time");
            entity.Property(e => e.PmrId).HasColumnName("pmr_id");
            entity.Property(e => e.PoolId).HasColumnName("pool_id");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");

            entity.HasOne(d => d.Pmr).WithMany(p => p.PoolOfflineFetchErrors)
                .HasForeignKey(d => d.PmrId)
                .HasConstraintName("pool_offline_fetch_error_pmr_id_fkey");

            entity.HasOne(d => d.Pool).WithMany(p => p.PoolOfflineFetchErrors)
                .HasForeignKey(d => d.PoolId)
                .HasConstraintName("pool_offline_fetch_error_pool_id_fkey");
        });

        modelBuilder.Entity<PoolOwner>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_owner_pkey");

            entity.ToTable("pool_owner");

            entity.HasIndex(e => e.PoolUpdateId, "pool_owner_pool_update_id_idx");

            entity.HasIndex(e => new { e.AddrId, e.PoolUpdateId }, "unique_pool_owner").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.PoolUpdateId).HasColumnName("pool_update_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.PoolOwners)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("pool_owner_addr_id_fkey");

            entity.HasOne(d => d.PoolUpdate).WithMany(p => p.PoolOwners)
                .HasForeignKey(d => d.PoolUpdateId)
                .HasConstraintName("pool_owner_pool_update_id_fkey");
        });

        modelBuilder.Entity<PoolRelay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_relay_pkey");

            entity.ToTable("pool_relay");

            entity.HasIndex(e => e.UpdateId, "idx_pool_relay_update_id");

            entity.HasIndex(e => new { e.UpdateId, e.Ipv4, e.Ipv6, e.DnsName }, "unique_pool_relay").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DnsName)
                .HasColumnType("character varying")
                .HasColumnName("dns_name");
            entity.Property(e => e.DnsSrvName)
                .HasColumnType("character varying")
                .HasColumnName("dns_srv_name");
            entity.Property(e => e.Ipv4)
                .HasColumnType("character varying")
                .HasColumnName("ipv4");
            entity.Property(e => e.Ipv6)
                .HasColumnType("character varying")
                .HasColumnName("ipv6");
            entity.Property(e => e.Port).HasColumnName("port");
            entity.Property(e => e.UpdateId).HasColumnName("update_id");

            entity.HasOne(d => d.Update).WithMany(p => p.PoolRelays)
                .HasForeignKey(d => d.UpdateId)
                .HasConstraintName("pool_relay_update_id_fkey");
        });

        modelBuilder.Entity<PoolRetire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_retire_pkey");

            entity.ToTable("pool_retire");

            entity.HasIndex(e => e.AnnouncedTxId, "idx_pool_retire_announced_tx_id");

            entity.HasIndex(e => e.HashId, "idx_pool_retire_hash_id");

            entity.HasIndex(e => new { e.AnnouncedTxId, e.CertIndex }, "unique_pool_retiring").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnnouncedTxId).HasColumnName("announced_tx_id");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.HashId).HasColumnName("hash_id");
            entity.Property(e => e.RetiringEpoch).HasColumnName("retiring_epoch");

            entity.HasOne(d => d.AnnouncedTx).WithMany(p => p.PoolRetires)
                .HasForeignKey(d => d.AnnouncedTxId)
                .HasConstraintName("pool_retire_announced_tx_id_fkey");

            entity.HasOne(d => d.Hash).WithMany(p => p.PoolRetires)
                .HasForeignKey(d => d.HashId)
                .HasConstraintName("pool_retire_hash_id_fkey");
        });

        modelBuilder.Entity<PoolUpdate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pool_update_pkey");

            entity.ToTable("pool_update");

            entity.HasIndex(e => e.ActiveEpochNo, "idx_pool_update_active_epoch_no");

            entity.HasIndex(e => e.HashId, "idx_pool_update_hash_id");

            entity.HasIndex(e => e.MetaId, "idx_pool_update_meta_id");

            entity.HasIndex(e => e.RegisteredTxId, "idx_pool_update_registered_tx_id");

            entity.HasIndex(e => e.RewardAddrId, "idx_pool_update_reward_addr");

            entity.HasIndex(e => new { e.RegisteredTxId, e.CertIndex }, "unique_pool_update").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActiveEpochNo).HasColumnName("active_epoch_no");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.FixedCost)
                .HasPrecision(20)
                .HasColumnName("fixed_cost");
            entity.Property(e => e.HashId).HasColumnName("hash_id");
            entity.Property(e => e.Margin).HasColumnName("margin");
            entity.Property(e => e.MetaId).HasColumnName("meta_id");
            entity.Property(e => e.Pledge)
                .HasPrecision(20)
                .HasColumnName("pledge");
            entity.Property(e => e.RegisteredTxId).HasColumnName("registered_tx_id");
            entity.Property(e => e.RewardAddrId).HasColumnName("reward_addr_id");
            entity.Property(e => e.VrfKeyHash).HasColumnName("vrf_key_hash");

            entity.HasOne(d => d.Hash).WithMany(p => p.PoolUpdates)
                .HasForeignKey(d => d.HashId)
                .HasConstraintName("pool_update_hash_id_fkey");

            entity.HasOne(d => d.Meta).WithMany(p => p.PoolUpdates)
                .HasForeignKey(d => d.MetaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("pool_update_meta_id_fkey");

            entity.HasOne(d => d.RegisteredTx).WithMany(p => p.PoolUpdates)
                .HasForeignKey(d => d.RegisteredTxId)
                .HasConstraintName("pool_update_registered_tx_id_fkey");

            entity.HasOne(d => d.RewardAddr).WithMany(p => p.PoolUpdates)
                .HasForeignKey(d => d.RewardAddrId)
                .HasConstraintName("pool_update_reward_addr_id_fkey");
        });

        modelBuilder.Entity<PotTransfer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pot_transfer_pkey");

            entity.ToTable("pot_transfer");

            entity.HasIndex(e => new { e.TxId, e.CertIndex }, "unique_pot_transfer").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.Reserves)
                .HasPrecision(20)
                .HasColumnName("reserves");
            entity.Property(e => e.Treasury)
                .HasPrecision(20)
                .HasColumnName("treasury");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Tx).WithMany(p => p.PotTransfers)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("pot_transfer_tx_id_fkey");
        });

        modelBuilder.Entity<Redeemer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("redeemer_pkey");

            entity.ToTable("redeemer");

            entity.HasIndex(e => e.RedeemerDataId, "redeemer_redeemer_data_id_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Fee)
                .HasPrecision(20)
                .HasColumnName("fee");
            entity.Property(e => e.Index).HasColumnName("index");
            entity.Property(e => e.RedeemerDataId).HasColumnName("redeemer_data_id");
            entity.Property(e => e.ScriptHash).HasColumnName("script_hash");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.UnitMem).HasColumnName("unit_mem");
            entity.Property(e => e.UnitSteps).HasColumnName("unit_steps");

            entity.HasOne(d => d.RedeemerData).WithMany(p => p.Redeemers)
                .HasForeignKey(d => d.RedeemerDataId)
                .HasConstraintName("redeemer_redeemer_data_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.Redeemers)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("redeemer_tx_id_fkey");
        });

        modelBuilder.Entity<RedeemerDatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("redeemer_data_pkey");

            entity.ToTable("redeemer_data");

            entity.HasIndex(e => e.TxId, "redeemer_data_tx_id_idx");

            entity.HasIndex(e => e.Hash, "unique_redeemer_data").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bytes).HasColumnName("bytes");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.Value)
                .HasColumnType("jsonb")
                .HasColumnName("value");

            entity.HasOne(d => d.Tx).WithMany(p => p.RedeemerData)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("redeemer_data_tx_id_fkey");
        });

        modelBuilder.Entity<ReferenceTxIn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reference_tx_in_pkey");

            entity.ToTable("reference_tx_in");

            entity.HasIndex(e => e.TxOutId, "reference_tx_in_tx_out_id_idx");

            entity.HasIndex(e => new { e.TxInId, e.TxOutId, e.TxOutIndex }, "unique_ref_txin").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TxInId).HasColumnName("tx_in_id");
            entity.Property(e => e.TxOutId).HasColumnName("tx_out_id");
            entity.Property(e => e.TxOutIndex).HasColumnName("tx_out_index");

            entity.HasOne(d => d.TxIn).WithMany(p => p.ReferenceTxInTxIns)
                .HasForeignKey(d => d.TxInId)
                .HasConstraintName("reference_tx_in_tx_in_id_fkey");

            entity.HasOne(d => d.TxOut).WithMany(p => p.ReferenceTxInTxOuts)
                .HasForeignKey(d => d.TxOutId)
                .HasConstraintName("reference_tx_in_tx_out_id_fkey");
        });

        modelBuilder.Entity<Reserve>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reserve_pkey");

            entity.ToTable("reserve");

            entity.HasIndex(e => e.AddrId, "idx_reserve_addr_id");

            entity.HasIndex(e => e.TxId, "idx_reserve_tx_id");

            entity.HasIndex(e => new { e.AddrId, e.TxId, e.CertIndex }, "unique_reserves").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.Amount)
                .HasPrecision(20)
                .HasColumnName("amount");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.Reserves)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("reserve_addr_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.Reserves)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("reserve_tx_id_fkey");
        });

        modelBuilder.Entity<ReservedPoolTicker>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reserved_pool_ticker_pkey");

            entity.ToTable("reserved_pool_ticker");

            entity.HasIndex(e => e.PoolHash, "idx_reserved_pool_ticker_pool_hash");

            entity.HasIndex(e => e.Name, "unique_reserved_pool_ticker").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.PoolHash).HasColumnName("pool_hash");
        });

        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reward_pkey");

            entity.ToTable("reward");

            entity.HasIndex(e => e.AddrId, "idx_reward_addr_id");

            entity.HasIndex(e => e.EarnedEpoch, "idx_reward_earned_epoch");

            entity.HasIndex(e => e.PoolId, "idx_reward_pool_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.Amount)
                .HasPrecision(20)
                .HasColumnName("amount");
            entity.Property(e => e.EarnedEpoch).HasColumnName("earned_epoch");
            entity.Property(e => e.PoolId).HasColumnName("pool_id");
            entity.Property(e => e.SpendableEpoch).HasColumnName("spendable_epoch");

            entity.HasOne(d => d.Addr).WithMany(p => p.Rewards)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("reward_addr_id_fkey");

            entity.HasOne(d => d.Pool).WithMany(p => p.Rewards)
                .HasForeignKey(d => d.PoolId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("reward_pool_id_fkey");
        });

        modelBuilder.Entity<SchemaVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("schema_version_pkey");

            entity.ToTable("schema_version");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StageOne).HasColumnName("stage_one");
            entity.Property(e => e.StageThree).HasColumnName("stage_three");
            entity.Property(e => e.StageTwo).HasColumnName("stage_two");
        });

        modelBuilder.Entity<Script>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("script_pkey");

            entity.ToTable("script");

            entity.HasIndex(e => e.TxId, "idx_script_tx_id");

            entity.HasIndex(e => e.Hash, "unique_script").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bytes).HasColumnName("bytes");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.Json)
                .HasColumnType("jsonb")
                .HasColumnName("json");
            entity.Property(e => e.SerialisedSize).HasColumnName("serialised_size");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Tx).WithMany(p => p.Scripts)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("script_tx_id_fkey");
        });

        modelBuilder.Entity<SlotLeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("slot_leader_pkey");

            entity.ToTable("slot_leader");

            entity.HasIndex(e => e.PoolHashId, "idx_slot_leader_pool_hash_id");

            entity.HasIndex(e => e.Hash, "unique_slot_leader").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.PoolHashId).HasColumnName("pool_hash_id");

            entity.HasOne(d => d.PoolHash).WithMany(p => p.SlotLeaders)
                .HasForeignKey(d => d.PoolHashId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("slot_leader_pool_hash_id_fkey");
        });

        modelBuilder.Entity<StakeAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stake_address_pkey");

            entity.ToTable("stake_address");

            entity.HasIndex(e => e.HashRaw, "idx_stake_address_hash_raw");

            entity.HasIndex(e => e.TxId, "idx_stake_address_registered_tx_id");

            entity.HasIndex(e => e.View, "idx_stake_address_view").HasMethod("hash");

            entity.HasIndex(e => e.HashRaw, "unique_stake_address").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HashRaw).HasColumnName("hash_raw");
            entity.Property(e => e.ScriptHash).HasColumnName("script_hash");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.View)
                .HasColumnType("character varying")
                .HasColumnName("view");

            entity.HasOne(d => d.Tx).WithMany(p => p.StakeAddresses)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("stake_address_tx_id_fkey");
        });

        modelBuilder.Entity<StakeDeregistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stake_deregistration_pkey");

            entity.ToTable("stake_deregistration");

            entity.HasIndex(e => e.AddrId, "idx_stake_deregistration_addr_id");

            entity.HasIndex(e => e.RedeemerId, "idx_stake_deregistration_redeemer_id");

            entity.HasIndex(e => e.TxId, "idx_stake_deregistration_tx_id");

            entity.HasIndex(e => new { e.TxId, e.CertIndex }, "unique_stake_deregistration").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.RedeemerId).HasColumnName("redeemer_id");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.StakeDeregistrations)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("stake_deregistration_addr_id_fkey");

            entity.HasOne(d => d.Redeemer).WithMany(p => p.StakeDeregistrations)
                .HasForeignKey(d => d.RedeemerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("stake_deregistration_redeemer_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.StakeDeregistrations)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("stake_deregistration_tx_id_fkey");
        });

        modelBuilder.Entity<StakeRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stake_registration_pkey");

            entity.ToTable("stake_registration");

            entity.HasIndex(e => e.AddrId, "idx_stake_registration_addr_id");

            entity.HasIndex(e => e.TxId, "idx_stake_registration_tx_id");

            entity.HasIndex(e => new { e.TxId, e.CertIndex }, "unique_stake_registration").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.EpochNo).HasColumnName("epoch_no");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.StakeRegistrations)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("stake_registration_addr_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.StakeRegistrations)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("stake_registration_tx_id_fkey");
        });

        modelBuilder.Entity<TableBloat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("table_bloat", "metric_helpers");

            entity.Property(e => e.TBloatRatio).HasColumnName("t_bloat_ratio");
            entity.Property(e => e.TBloatSize).HasColumnName("t_bloat_size");
            entity.Property(e => e.TExtraRatio).HasColumnName("t_extra_ratio");
            entity.Property(e => e.TExtraSize).HasColumnName("t_extra_size");
            entity.Property(e => e.TFillFactor).HasColumnName("t_fill_factor");
            entity.Property(e => e.TIsNa).HasColumnName("t_is_na");
            entity.Property(e => e.TRealSize).HasColumnName("t_real_size");
        });

        modelBuilder.Entity<Treasury>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("treasury_pkey");

            entity.ToTable("treasury");

            entity.HasIndex(e => e.AddrId, "idx_treasury_addr_id");

            entity.HasIndex(e => e.TxId, "idx_treasury_tx_id");

            entity.HasIndex(e => new { e.AddrId, e.TxId, e.CertIndex }, "unique_treasury").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.Amount)
                .HasPrecision(20)
                .HasColumnName("amount");
            entity.Property(e => e.CertIndex).HasColumnName("cert_index");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.Treasuries)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("treasury_addr_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.Treasuries)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("treasury_tx_id_fkey");
        });

        modelBuilder.Entity<Tx>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tx_pkey");

            entity.ToTable("tx");

            entity.HasIndex(e => e.BlockId, "idx_tx_block_id");

            entity.HasIndex(e => e.Hash, "unique_tx").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockId).HasColumnName("block_id");
            entity.Property(e => e.BlockIndex).HasColumnName("block_index");
            entity.Property(e => e.Deposit).HasColumnName("deposit");
            entity.Property(e => e.Fee)
                .HasPrecision(20)
                .HasColumnName("fee");
            entity.Property(e => e.Hash).HasColumnName("hash");
            entity.Property(e => e.InvalidBefore)
                .HasPrecision(20)
                .HasColumnName("invalid_before");
            entity.Property(e => e.InvalidHereafter)
                .HasPrecision(20)
                .HasColumnName("invalid_hereafter");
            entity.Property(e => e.OutSum)
                .HasPrecision(20)
                .HasColumnName("out_sum");
            entity.Property(e => e.ScriptSize).HasColumnName("script_size");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.ValidContract).HasColumnName("valid_contract");

            entity.HasOne(d => d.Block).WithMany(p => p.Txes)
                .HasForeignKey(d => d.BlockId)
                .HasConstraintName("tx_block_id_fkey");
        });

        modelBuilder.Entity<TxIn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tx_in_pkey");

            entity.ToTable("tx_in");

            entity.HasIndex(e => e.RedeemerId, "idx_tx_in_redeemer_id");

            entity.HasIndex(e => e.TxInId, "idx_tx_in_source_tx");

            entity.HasIndex(e => e.TxInId, "idx_tx_in_tx_in_id");

            entity.HasIndex(e => e.TxOutId, "idx_tx_in_tx_out_id");

            entity.HasIndex(e => new { e.TxOutId, e.TxOutIndex }, "unique_txin").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RedeemerId).HasColumnName("redeemer_id");
            entity.Property(e => e.TxInId).HasColumnName("tx_in_id");
            entity.Property(e => e.TxOutId).HasColumnName("tx_out_id");
            entity.Property(e => e.TxOutIndex).HasColumnName("tx_out_index");

            entity.HasOne(d => d.Redeemer).WithMany(p => p.TxIns)
                .HasForeignKey(d => d.RedeemerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tx_in_redeemer_id_fkey");

            entity.HasOne(d => d.TxInNavigation).WithMany(p => p.TxInTxInNavigations)
                .HasForeignKey(d => d.TxInId)
                .HasConstraintName("tx_in_tx_in_id_fkey");

            entity.HasOne(d => d.TxOut).WithMany(p => p.TxInTxOuts)
                .HasForeignKey(d => d.TxOutId)
                .HasConstraintName("tx_in_tx_out_id_fkey");
        });

        modelBuilder.Entity<TxMetadatum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tx_metadata_pkey");

            entity.ToTable("tx_metadata");

            entity.HasIndex(e => e.TxId, "idx_tx_metadata_tx_id");

            entity.HasIndex(e => new { e.Key, e.TxId }, "unique_tx_metadata").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bytes).HasColumnName("bytes");
            entity.Property(e => e.Json)
                .HasColumnType("jsonb")
                .HasColumnName("json");
            entity.Property(e => e.Key)
                .HasPrecision(20)
                .HasColumnName("key");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Tx).WithMany(p => p.TxMetadata)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("tx_metadata_tx_id_fkey");
        });

        modelBuilder.Entity<TxOut>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tx_out_pkey");

            entity.ToTable("tx_out");

            entity.HasIndex(e => e.Address, "idx_tx_out_address").HasMethod("hash");

            entity.HasIndex(e => e.PaymentCred, "idx_tx_out_payment_cred");

            entity.HasIndex(e => e.StakeAddressId, "idx_tx_out_stake_address_id");

            entity.HasIndex(e => e.TxId, "idx_tx_out_tx_id");

            entity.HasIndex(e => e.InlineDatumId, "tx_out_inline_datum_id_idx");

            entity.HasIndex(e => e.ReferenceScriptId, "tx_out_reference_script_id_idx");

            entity.HasIndex(e => new { e.TxId, e.Index }, "unique_txout").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.AddressHasScript).HasColumnName("address_has_script");
            entity.Property(e => e.AddressRaw).HasColumnName("address_raw");
            entity.Property(e => e.DataHash).HasColumnName("data_hash");
            entity.Property(e => e.Index).HasColumnName("index");
            entity.Property(e => e.InlineDatumId).HasColumnName("inline_datum_id");
            entity.Property(e => e.PaymentCred).HasColumnName("payment_cred");
            entity.Property(e => e.ReferenceScriptId).HasColumnName("reference_script_id");
            entity.Property(e => e.StakeAddressId).HasColumnName("stake_address_id");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.Value)
                .HasPrecision(20)
                .HasColumnName("value");

            entity.HasOne(d => d.InlineDatum).WithMany(p => p.TxOuts)
                .HasForeignKey(d => d.InlineDatumId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tx_out_inline_datum_id_fkey");

            entity.HasOne(d => d.ReferenceScript).WithMany(p => p.TxOuts)
                .HasForeignKey(d => d.ReferenceScriptId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tx_out_reference_script_id_fkey");

            entity.HasOne(d => d.StakeAddress).WithMany(p => p.TxOuts)
                .HasForeignKey(d => d.StakeAddressId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("tx_out_stake_address_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.TxOuts)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("tx_out_tx_id_fkey");
        });

        modelBuilder.Entity<UtxoByronView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("utxo_byron_view");

            entity.Property(e => e.Address)
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.AddressHasScript).HasColumnName("address_has_script");
            entity.Property(e => e.AddressRaw).HasColumnName("address_raw");
            entity.Property(e => e.DataHash).HasColumnName("data_hash");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Index).HasColumnName("index");
            entity.Property(e => e.InlineDatumId).HasColumnName("inline_datum_id");
            entity.Property(e => e.PaymentCred).HasColumnName("payment_cred");
            entity.Property(e => e.ReferenceScriptId).HasColumnName("reference_script_id");
            entity.Property(e => e.StakeAddressId).HasColumnName("stake_address_id");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.Value)
                .HasPrecision(20)
                .HasColumnName("value");
        });

        modelBuilder.Entity<UtxoView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("utxo_view");

            entity.Property(e => e.Address)
                .HasColumnType("character varying")
                .HasColumnName("address");
            entity.Property(e => e.AddressHasScript).HasColumnName("address_has_script");
            entity.Property(e => e.AddressRaw).HasColumnName("address_raw");
            entity.Property(e => e.DataHash).HasColumnName("data_hash");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Index).HasColumnName("index");
            entity.Property(e => e.InlineDatumId).HasColumnName("inline_datum_id");
            entity.Property(e => e.PaymentCred).HasColumnName("payment_cred");
            entity.Property(e => e.ReferenceScriptId).HasColumnName("reference_script_id");
            entity.Property(e => e.StakeAddressId).HasColumnName("stake_address_id");
            entity.Property(e => e.TxId).HasColumnName("tx_id");
            entity.Property(e => e.Value)
                .HasPrecision(20)
                .HasColumnName("value");
        });

        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("withdrawal_pkey");

            entity.ToTable("withdrawal");

            entity.HasIndex(e => e.AddrId, "idx_withdrawal_addr_id");

            entity.HasIndex(e => e.RedeemerId, "idx_withdrawal_redeemer_id");

            entity.HasIndex(e => e.TxId, "idx_withdrawal_tx_id");

            entity.HasIndex(e => new { e.AddrId, e.TxId }, "unique_withdrawal").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddrId).HasColumnName("addr_id");
            entity.Property(e => e.Amount)
                .HasPrecision(20)
                .HasColumnName("amount");
            entity.Property(e => e.RedeemerId).HasColumnName("redeemer_id");
            entity.Property(e => e.TxId).HasColumnName("tx_id");

            entity.HasOne(d => d.Addr).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.AddrId)
                .HasConstraintName("withdrawal_addr_id_fkey");

            entity.HasOne(d => d.Redeemer).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.RedeemerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("withdrawal_redeemer_id_fkey");

            entity.HasOne(d => d.Tx).WithMany(p => p.Withdrawals)
                .HasForeignKey(d => d.TxId)
                .HasConstraintName("withdrawal_tx_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

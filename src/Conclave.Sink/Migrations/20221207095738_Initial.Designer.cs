﻿// <auto-generated />
using System.Collections.Generic;
using System.Text.Json;
using Conclave.Sink.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Conclave.Sink.Migrations
{
    [DbContext(typeof(ConclaveSinkDbContext))]
    [Migration("20221207095738_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Conclave.Common.Models.AddressByStake", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<List<string>>("PaymentAddresses")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.HasKey("StakeAddress");

                    b.ToTable("AddressByStake");
                });

            modelBuilder.Entity("Conclave.Common.Models.Asset", b =>
                {
                    b.Property<string>("PolicyId")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("TxOutputHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxOutputIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("PolicyId", "Name", "TxOutputHash", "TxOutputIndex");

                    b.HasIndex("TxOutputHash", "TxOutputIndex");

                    b.ToTable("Assets");
                });

            modelBuilder.Entity("Conclave.Common.Models.BalanceByAddress", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Address");

                    b.ToTable("BalanceByAddress");
                });

            modelBuilder.Entity("Conclave.Common.Models.BalanceByStakeEpoch", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<decimal>("Epoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("StakeAddress", "Epoch");

                    b.ToTable("BalanceByStakeEpoch");
                });

            modelBuilder.Entity("Conclave.Common.Models.Block", b =>
                {
                    b.Property<string>("BlockHash")
                        .HasColumnType("text");

                    b.Property<decimal>("BlockNumber")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Epoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Era")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("VrfKeyhash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("BlockHash");

                    b.ToTable("Blocks");
                });

            modelBuilder.Entity("Conclave.Common.Models.CnclvByStakeEpoch", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<decimal>("Epoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("StakeAddress", "Epoch");

                    b.ToTable("CnclvByStakeEpoch");
                });

            modelBuilder.Entity("Conclave.Common.Models.PoolRegistration", b =>
                {
                    b.Property<string>("PoolId")
                        .HasColumnType("text");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("Cost")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Margin")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Pledge")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("PoolMetadataHash")
                        .HasColumnType("text");

                    b.Property<JsonDocument>("PoolMetadataJSON")
                        .HasColumnType("jsonb");

                    b.Property<string>("PoolMetadataString")
                        .HasColumnType("text");

                    b.Property<List<string>>("PoolOwners")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<List<string>>("Relays")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<string>("RewardAccount")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TransactionHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VrfKeyHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PoolId", "TxHash");

                    b.HasIndex("TransactionHash");

                    b.ToTable("PoolRegistrations");
                });

            modelBuilder.Entity("Conclave.Common.Models.PoolRetirement", b =>
                {
                    b.Property<string>("Pool")
                        .HasColumnType("text");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("EffectiveEpoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("TransactionHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Pool", "TxHash");

                    b.HasIndex("TransactionHash");

                    b.ToTable("PoolRetirements");
                });

            modelBuilder.Entity("Conclave.Common.Models.StakeDelegation", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<string>("PoolId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("StakeAddress", "TxHash");

                    b.HasIndex("TxHash");

                    b.ToTable("StakeDelegations");
                });

            modelBuilder.Entity("Conclave.Common.Models.Transaction", b =>
                {
                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("BlockHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Fee")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Hash");

                    b.HasIndex("BlockHash");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Conclave.Common.Models.TxInput", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<string>("TxOutputHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxOutputIndex")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("TxHash", "TxOutputHash", "TxOutputIndex");

                    b.HasIndex("TxOutputHash", "TxOutputIndex");

                    b.ToTable("TxInputs");
                });

            modelBuilder.Entity("Conclave.Common.Models.TxOutput", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("Index")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("TxHash", "Index");

                    b.ToTable("TxOutputs");
                });

            modelBuilder.Entity("Conclave.Common.Models.Withdrawal", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("TxHash", "StakeAddress");

                    b.ToTable("Withdrawals");
                });

            modelBuilder.Entity("Conclave.Common.Models.WithdrawalByStakeEpoch", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<decimal>("Epoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("StakeAddress", "Epoch");

                    b.ToTable("WithdrawalByStakeEpoch");
                });

            modelBuilder.Entity("Conclave.Common.Models.Asset", b =>
                {
                    b.HasOne("Conclave.Common.Models.TxOutput", "TxOutput")
                        .WithMany("Assets")
                        .HasForeignKey("TxOutputHash", "TxOutputIndex")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TxOutput");
                });

            modelBuilder.Entity("Conclave.Common.Models.PoolRegistration", b =>
                {
                    b.HasOne("Conclave.Common.Models.Transaction", "Transaction")
                        .WithMany("PoolRegistrations")
                        .HasForeignKey("TransactionHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("Conclave.Common.Models.PoolRetirement", b =>
                {
                    b.HasOne("Conclave.Common.Models.Transaction", "Transaction")
                        .WithMany("PoolRetirements")
                        .HasForeignKey("TransactionHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("Conclave.Common.Models.StakeDelegation", b =>
                {
                    b.HasOne("Conclave.Common.Models.Transaction", "Transaction")
                        .WithMany("StakeDelegations")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("Conclave.Common.Models.Transaction", b =>
                {
                    b.HasOne("Conclave.Common.Models.Block", "Block")
                        .WithMany("Transactions")
                        .HasForeignKey("BlockHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Block");
                });

            modelBuilder.Entity("Conclave.Common.Models.TxInput", b =>
                {
                    b.HasOne("Conclave.Common.Models.Transaction", "Transaction")
                        .WithMany("Inputs")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Conclave.Common.Models.TxOutput", "TxOutput")
                        .WithMany("Inputs")
                        .HasForeignKey("TxOutputHash", "TxOutputIndex")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");

                    b.Navigation("TxOutput");
                });

            modelBuilder.Entity("Conclave.Common.Models.TxOutput", b =>
                {
                    b.HasOne("Conclave.Common.Models.Transaction", "Transaction")
                        .WithMany("Outputs")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("Conclave.Common.Models.Withdrawal", b =>
                {
                    b.HasOne("Conclave.Common.Models.Transaction", "Transaction")
                        .WithMany("Withdrawals")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("Conclave.Common.Models.Block", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("Conclave.Common.Models.Transaction", b =>
                {
                    b.Navigation("Inputs");

                    b.Navigation("Outputs");

                    b.Navigation("PoolRegistrations");

                    b.Navigation("PoolRetirements");

                    b.Navigation("StakeDelegations");

                    b.Navigation("Withdrawals");
                });

            modelBuilder.Entity("Conclave.Common.Models.TxOutput", b =>
                {
                    b.Navigation("Assets");

                    b.Navigation("Inputs");
                });
#pragma warning restore 612, 618
        }
    }
}

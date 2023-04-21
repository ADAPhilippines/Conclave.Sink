﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TeddySwap.Sink.Data;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations.TeddySwapNftSinkDb
{
    [DbContext(typeof(TeddySwapNftSinkDbContext))]
    partial class TeddySwapNftSinkDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TeddySwap.Common.Models.Asset", b =>
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

            modelBuilder.Entity("TeddySwap.Common.Models.Block", b =>
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

                    b.Property<IEnumerable<ulong>>("InvalidTransactions")
                        .HasColumnType("jsonb");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("VrfKeyhash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("BlockHash");

                    b.ToTable("Blocks");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.CollateralTxIn", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<string>("TxOutputHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxOutputIndex")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("TxHash", "TxOutputHash", "TxOutputIndex");

                    b.ToTable("CollateralTxIns");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.CollateralTxOut", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Index")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Address", "TxHash");

                    b.HasIndex("TxHash")
                        .IsUnique();

                    b.ToTable("CollateralTxOuts");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.MintTransaction", b =>
                {
                    b.Property<string>("PolicyId")
                        .HasColumnType("text");

                    b.Property<string>("TokenName")
                        .HasColumnType("text");

                    b.Property<string>("AsciiTokenName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Metadata")
                        .HasColumnType("text");

                    b.Property<string>("TransactionHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PolicyId", "TokenName");

                    b.HasIndex("TransactionHash");

                    b.ToTable("MintTransactions");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.NftOwner", b =>
                {
                    b.Property<string>("PolicyId")
                        .HasColumnType("text");

                    b.Property<string>("TokenName")
                        .HasColumnType("text");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("StakeAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PolicyId", "TokenName");

                    b.ToTable("NftOwners");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.Transaction", b =>
                {
                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("Blockhash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Fee")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("HasCollateralOutput")
                        .HasColumnType("boolean");

                    b.Property<decimal>("Index")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Metadata")
                        .HasColumnType("text");

                    b.HasKey("Hash");

                    b.HasIndex("Blockhash");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.TxInput", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<string>("TxOutputHash")
                        .HasColumnType("text");

                    b.Property<decimal>("TxOutputIndex")
                        .HasColumnType("numeric(20,0)");

                    b.Property<byte?>("InlineDatum")
                        .HasColumnType("smallint");

                    b.HasKey("TxHash", "TxOutputHash", "TxOutputIndex");

                    b.HasIndex("TxOutputHash", "TxOutputIndex");

                    b.ToTable("TxInputs");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.TxOutput", b =>
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

                    b.Property<string>("DatumCbor")
                        .HasColumnType("text");

                    b.Property<decimal>("TxIndex")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("TxHash", "Index");

                    b.ToTable("TxOutputs");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.Asset", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.TxOutput", "TxOutput")
                        .WithMany("Assets")
                        .HasForeignKey("TxOutputHash", "TxOutputIndex")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TxOutput");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.CollateralTxIn", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.Transaction", "Transaction")
                        .WithMany("CollateralTxIns")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.CollateralTxOut", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.Transaction", "Transaction")
                        .WithOne("CollateralTxOut")
                        .HasForeignKey("TeddySwap.Common.Models.CollateralTxOut", "TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.MintTransaction", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.Transaction", "Transaction")
                        .WithMany()
                        .HasForeignKey("TransactionHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.Transaction", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.Block", "Block")
                        .WithMany("Transactions")
                        .HasForeignKey("Blockhash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Block");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.TxInput", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.Transaction", "Transaction")
                        .WithMany("Inputs")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TeddySwap.Common.Models.TxOutput", "TxOutput")
                        .WithMany("Inputs")
                        .HasForeignKey("TxOutputHash", "TxOutputIndex")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");

                    b.Navigation("TxOutput");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.TxOutput", b =>
                {
                    b.HasOne("TeddySwap.Common.Models.Transaction", "Transaction")
                        .WithMany("Outputs")
                        .HasForeignKey("TxHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.Block", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.Transaction", b =>
                {
                    b.Navigation("CollateralTxIns");

                    b.Navigation("CollateralTxOut");

                    b.Navigation("Inputs");

                    b.Navigation("Outputs");
                });

            modelBuilder.Entity("TeddySwap.Common.Models.TxOutput", b =>
                {
                    b.Navigation("Assets");

                    b.Navigation("Inputs");
                });
#pragma warning restore 612, 618
        }
    }
}

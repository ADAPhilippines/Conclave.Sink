﻿// <auto-generated />
using System.Collections.Generic;
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
    [Migration("20221201100222_edited poolregistered models")]
    partial class editedpoolregisteredmodels
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Conclave.Sink.Models.AddressByStake", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<List<string>>("PaymentAddresses")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.HasKey("StakeAddress");

                    b.ToTable("AddressByStake");
                });

            modelBuilder.Entity("Conclave.Sink.Models.BalanceByAddress", b =>
                {
                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Address");

                    b.ToTable("BalanceByAddress");
                });

            modelBuilder.Entity("Conclave.Sink.Models.BalanceByStakeAddressEpoch", b =>
                {
                    b.Property<string>("StakeAddress")
                        .HasColumnType("text");

                    b.Property<decimal>("Epoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Balance")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("StakeAddress", "Epoch");

                    b.ToTable("BalanceByStakeAddressEpoch");
                });

            modelBuilder.Entity("Conclave.Sink.Models.Block", b =>
                {
                    b.Property<string>("BlockHash")
                        .HasColumnType("text");

                    b.Property<decimal>("BlockNumber")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Epoch")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("BlockHash");

                    b.ToTable("Block");
                });

            modelBuilder.Entity("Conclave.Sink.Models.Pool", b =>
                {
                    b.Property<string>("Operator")
                        .HasColumnType("text");

                    b.Property<decimal>("Cost")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Margin")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Pledge")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("PoolMetadata")
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

                    b.Property<string>("VRFKeyHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Operator");

                    b.ToTable("Pools");
                });

            modelBuilder.Entity("Conclave.Sink.Models.TxInput", b =>
                {
                    b.Property<string>("TxHash")
                        .HasColumnType("text");

                    b.Property<decimal>("Index")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Slot")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("BlockHash")
                        .HasColumnType("text");

                    b.HasKey("TxHash", "Index", "Slot");

                    b.HasIndex("BlockHash");

                    b.ToTable("TxInput");
                });

            modelBuilder.Entity("Conclave.Sink.Models.TxOutput", b =>
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

                    b.Property<string>("BlockHash")
                        .HasColumnType("text");

                    b.HasKey("TxHash", "Index");

                    b.HasIndex("BlockHash");

                    b.ToTable("TxOutput");
                });

            modelBuilder.Entity("Conclave.Sink.Models.TxInput", b =>
                {
                    b.HasOne("Conclave.Sink.Models.Block", "Block")
                        .WithMany()
                        .HasForeignKey("BlockHash");

                    b.Navigation("Block");
                });

            modelBuilder.Entity("Conclave.Sink.Models.TxOutput", b =>
                {
                    b.HasOne("Conclave.Sink.Models.Block", "Block")
                        .WithMany()
                        .HasForeignKey("BlockHash");

                    b.Navigation("Block");
                });
#pragma warning restore 612, 618
        }
    }
}

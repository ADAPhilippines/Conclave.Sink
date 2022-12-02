using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressByStake",
                columns: table => new
                {
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    PaymentAddresses = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressByStake", x => x.StakeAddress);
                });

            migrationBuilder.CreateTable(
                name: "BalanceByAddress",
                columns: table => new
                {
                    Address = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceByAddress", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "BalanceByStakeAddressEpoch",
                columns: table => new
                {
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceByStakeAddressEpoch", x => new { x.StakeAddress, x.Epoch });
                });

            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    BlockHash = table.Column<string>(type: "text", nullable: false),
                    BlockNumber = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.BlockHash);
                });

            migrationBuilder.CreateTable(
                name: "DelegatorByEpoch",
                columns: table => new
                {
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    PoolId = table.Column<string>(type: "text", nullable: false),
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BlockHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DelegatorByEpoch", x => new { x.StakeAddress, x.PoolId, x.TxHash, x.TxIndex });
                    table.ForeignKey(
                        name: "FK_DelegatorByEpoch_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "BlockHash");
                });

            migrationBuilder.CreateTable(
                name: "Pools",
                columns: table => new
                {
                    Operator = table.Column<string>(type: "text", nullable: false),
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    VRFKeyHash = table.Column<string>(type: "text", nullable: false),
                    Pledge = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Margin = table.Column<decimal>(type: "numeric", nullable: false),
                    RewardAccount = table.Column<string>(type: "text", nullable: false),
                    PoolOwners = table.Column<List<string>>(type: "text[]", nullable: false),
                    Relays = table.Column<List<string>>(type: "text[]", nullable: false),
                    PoolMetadata = table.Column<string>(type: "text", nullable: false),
                    BlockHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pools", x => new { x.Operator, x.TxHash });
                    table.ForeignKey(
                        name: "FK_Pools_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "BlockHash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardAddressByPoolPerEpoch",
                columns: table => new
                {
                    PoolId = table.Column<string>(type: "text", nullable: false),
                    RewardAddress = table.Column<string>(type: "text", nullable: false),
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BlockHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardAddressByPoolPerEpoch", x => new { x.PoolId, x.RewardAddress, x.TxHash, x.TxIndex });
                    table.ForeignKey(
                        name: "FK_RewardAddressByPoolPerEpoch_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "BlockHash");
                });

            migrationBuilder.CreateTable(
                name: "TxOutput",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    BlockHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TxOutput", x => new { x.TxHash, x.Index });
                    table.ForeignKey(
                        name: "FK_TxOutput_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "BlockHash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TxInput",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxOutputHash = table.Column<string>(type: "text", nullable: false),
                    TxOutputIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BlockHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TxInput", x => new { x.TxHash, x.TxOutputHash, x.TxOutputIndex });
                    table.ForeignKey(
                        name: "FK_TxInput_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "BlockHash",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TxInput_TxOutput_TxOutputHash_TxOutputIndex",
                        columns: x => new { x.TxOutputHash, x.TxOutputIndex },
                        principalTable: "TxOutput",
                        principalColumns: new[] { "TxHash", "Index" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DelegatorByEpoch_BlockHash",
                table: "DelegatorByEpoch",
                column: "BlockHash");

            migrationBuilder.CreateIndex(
                name: "IX_Pools_BlockHash",
                table: "Pools",
                column: "BlockHash");

            migrationBuilder.CreateIndex(
                name: "IX_RewardAddressByPoolPerEpoch_BlockHash",
                table: "RewardAddressByPoolPerEpoch",
                column: "BlockHash");

            migrationBuilder.CreateIndex(
                name: "IX_TxInput_BlockHash",
                table: "TxInput",
                column: "BlockHash");

            migrationBuilder.CreateIndex(
                name: "IX_TxInput_TxOutputHash_TxOutputIndex",
                table: "TxInput",
                columns: new[] { "TxOutputHash", "TxOutputIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_TxOutput_BlockHash",
                table: "TxOutput",
                column: "BlockHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressByStake");

            migrationBuilder.DropTable(
                name: "BalanceByAddress");

            migrationBuilder.DropTable(
                name: "BalanceByStakeAddressEpoch");

            migrationBuilder.DropTable(
                name: "DelegatorByEpoch");

            migrationBuilder.DropTable(
                name: "Pools");

            migrationBuilder.DropTable(
                name: "RewardAddressByPoolPerEpoch");

            migrationBuilder.DropTable(
                name: "TxInput");

            migrationBuilder.DropTable(
                name: "TxOutput");

            migrationBuilder.DropTable(
                name: "Block");
        }
    }
}

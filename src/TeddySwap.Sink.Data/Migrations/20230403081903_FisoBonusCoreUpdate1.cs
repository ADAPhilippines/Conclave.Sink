using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations
{
    /// <inheritdoc />
    public partial class FisoBonusCoreUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FisoBonusDelegations",
                table: "FisoBonusDelegations");

            migrationBuilder.DropColumn(
                name: "HasBonus",
                table: "FisoDelegators");

            migrationBuilder.AddColumn<string>(
                name: "TxHash",
                table: "FisoBonusDelegations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Slot",
                table: "FisoBonusDelegations",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BlockNumber",
                table: "FisoBonusDelegations",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FisoBonusDelegations",
                table: "FisoBonusDelegations",
                columns: new[] { "StakeAddress", "TxHash", "Slot" });

            migrationBuilder.CreateTable(
                name: "CollateralTxIns",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    TxOutputHash = table.Column<string>(type: "text", nullable: false),
                    TxOutputIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralTxIns", x => new { x.TxHash, x.TxOutputHash, x.TxOutputIndex });
                    table.ForeignKey(
                        name: "FK_CollateralTxIns_Transactions_TxHash",
                        column: x => x.TxHash,
                        principalTable: "Transactions",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CollateralTxOuts",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    TxIndex = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Index = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollateralTxOuts", x => new { x.Address, x.TxHash });
                    table.ForeignKey(
                        name: "FK_CollateralTxOuts_Transactions_TxHash",
                        column: x => x.TxHash,
                        principalTable: "Transactions",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FisoDelegations",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    EpochNumber = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BlockNumber = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FromPoolId = table.Column<string>(type: "text", nullable: true),
                    ToPoolId = table.Column<string>(type: "text", nullable: false),
                    LiveStake = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FisoDelegations", x => new { x.StakeAddress, x.TxHash, x.Slot });
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollateralTxOuts_TxHash",
                table: "CollateralTxOuts",
                column: "TxHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollateralTxIns");

            migrationBuilder.DropTable(
                name: "CollateralTxOuts");

            migrationBuilder.DropTable(
                name: "FisoDelegations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FisoBonusDelegations",
                table: "FisoBonusDelegations");

            migrationBuilder.DropColumn(
                name: "TxHash",
                table: "FisoBonusDelegations");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "FisoBonusDelegations");

            migrationBuilder.DropColumn(
                name: "BlockNumber",
                table: "FisoBonusDelegations");

            migrationBuilder.AddColumn<bool>(
                name: "HasBonus",
                table: "FisoDelegators",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FisoBonusDelegations",
                table: "FisoBonusDelegations",
                columns: new[] { "EpochNumber", "PoolId", "StakeAddress" });
        }
    }
}

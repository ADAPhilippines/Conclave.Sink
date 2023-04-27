using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBonusField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceByStakeEpoch");

            migrationBuilder.DropTable(
                name: "WithdrawalByStakeEpoch");

            migrationBuilder.DropTable(
                name: "Withdrawals");

            migrationBuilder.DropColumn(
                name: "BonusAmount",
                table: "FisoEpochRewards");

            migrationBuilder.DropColumn(
                name: "HasBonus",
                table: "FisoEpochRewards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BonusAmount",
                table: "FisoEpochRewards",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "HasBonus",
                table: "FisoEpochRewards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BalanceByStakeEpoch",
                columns: table => new
                {
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceByStakeEpoch", x => new { x.StakeAddress, x.Epoch });
                });

            migrationBuilder.CreateTable(
                name: "WithdrawalByStakeEpoch",
                columns: table => new
                {
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalByStakeEpoch", x => new { x.StakeAddress, x.Epoch });
                });

            migrationBuilder.CreateTable(
                name: "Withdrawals",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdrawals", x => new { x.TxHash, x.StakeAddress });
                    table.ForeignKey(
                        name: "FK_Withdrawals_Transactions_TxHash",
                        column: x => x.TxHash,
                        principalTable: "Transactions",
                        principalColumn: "Hash",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}

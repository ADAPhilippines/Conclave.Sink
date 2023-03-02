using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Blocks_BlockHash",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "BlockHash",
                table: "Transactions",
                newName: "Blockhash");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BlockHash",
                table: "Transactions",
                newName: "IX_Transactions_Blockhash");

            migrationBuilder.CreateTable(
                name: "AddressVerifications",
                columns: table => new
                {
                    TestnetAddress = table.Column<string>(type: "text", nullable: false),
                    MainnetAddress = table.Column<string>(type: "text", nullable: false),
                    TestnetSignedData = table.Column<string>(type: "text", nullable: false),
                    MainnetSignedData = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressVerifications", x => x.TestnetAddress);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Blocks_Blockhash",
                table: "Transactions",
                column: "Blockhash",
                principalTable: "Blocks",
                principalColumn: "BlockHash",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Blocks_Blockhash",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "AddressVerifications");

            migrationBuilder.RenameColumn(
                name: "Blockhash",
                table: "Transactions",
                newName: "BlockHash");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Blockhash",
                table: "Transactions",
                newName: "IX_Transactions_BlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Blocks_BlockHash",
                table: "Transactions",
                column: "BlockHash",
                principalTable: "Blocks",
                principalColumn: "BlockHash",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

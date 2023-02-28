using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prices_Orders_OrderHash_OrderIndex",
                table: "Prices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderHash",
                table: "Prices",
                newName: "OrderTxHash");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "Prices",
                newName: "TxHash");

            migrationBuilder.RenameIndex(
                name: "IX_Prices_OrderHash_OrderIndex",
                table: "Prices",
                newName: "IX_Prices_OrderTxHash_OrderIndex");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "Orders",
                newName: "datum");

            migrationBuilder.AddColumn<string>(
                name: "TxHash",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                columns: new[] { "TxHash", "Index" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Transactions_TxHash",
                table: "Orders",
                column: "TxHash",
                principalTable: "Transactions",
                principalColumn: "Hash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Prices_Orders_OrderTxHash_OrderIndex",
                table: "Prices",
                columns: new[] { "OrderTxHash", "OrderIndex" },
                principalTable: "Orders",
                principalColumns: new[] { "TxHash", "Index" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Transactions_TxHash",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Prices_Orders_OrderTxHash_OrderIndex",
                table: "Prices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orders",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TxHash",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderTxHash",
                table: "Prices",
                newName: "OrderHash");

            migrationBuilder.RenameColumn(
                name: "TxHash",
                table: "Prices",
                newName: "Hash");

            migrationBuilder.RenameIndex(
                name: "IX_Prices_OrderTxHash_OrderIndex",
                table: "Prices",
                newName: "IX_Prices_OrderHash_OrderIndex");

            migrationBuilder.RenameColumn(
                name: "datum",
                table: "Orders",
                newName: "Hash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orders",
                table: "Orders",
                columns: new[] { "Hash", "Index" });

            migrationBuilder.AddForeignKey(
                name: "FK_Prices_Orders_OrderHash_OrderIndex",
                table: "Prices",
                columns: new[] { "OrderHash", "OrderIndex" },
                principalTable: "Orders",
                principalColumns: new[] { "Hash", "Index" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}

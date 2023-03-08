using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MainnetAddress",
                table: "AddressVerifications",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BatcherAddress",
                table: "Orders",
                column: "BatcherAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderType",
                table: "Orders",
                column: "OrderType");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Slot",
                table: "Orders",
                column: "Slot");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserAddress",
                table: "Orders",
                column: "UserAddress");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_BatcherAddress",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderType",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_Slot",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserAddress",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "MainnetAddress",
                table: "AddressVerifications",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}

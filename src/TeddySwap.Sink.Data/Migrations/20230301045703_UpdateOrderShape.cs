using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "datum",
                table: "Orders",
                newName: "OrderBase");

            migrationBuilder.AddColumn<byte[]>(
                name: "OrderDatum",
                table: "Orders",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PoolDatum",
                table: "Orders",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDatum",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PoolDatum",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "OrderBase",
                table: "Orders",
                newName: "datum");
        }
    }
}

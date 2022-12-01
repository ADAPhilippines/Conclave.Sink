using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class NewTxInputPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.AddColumn<decimal>(
                name: "Slot",
                table: "TxInput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                columns: new[] { "TxHash", "Index", "Slot" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "TxInput");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                columns: new[] { "TxHash", "Index" });
        }
    }
}

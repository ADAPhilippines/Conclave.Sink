using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class NewFieldsTxOutput : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BlockHash",
                table: "TxOutput",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "BlockNumber",
                table: "TxOutput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Epoch",
                table: "TxOutput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Slot",
                table: "TxOutput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockHash",
                table: "TxOutput");

            migrationBuilder.DropColumn(
                name: "BlockNumber",
                table: "TxOutput");

            migrationBuilder.DropColumn(
                name: "Epoch",
                table: "TxOutput");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "TxOutput");
        }
    }
}

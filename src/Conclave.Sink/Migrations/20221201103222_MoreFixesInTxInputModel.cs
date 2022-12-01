using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class MoreFixesInTxInputModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                columns: new[] { "TxHash", "TxInputOutputHash", "TxInputOutputIndex", "Slot" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                column: "TxHash");
        }
    }
}

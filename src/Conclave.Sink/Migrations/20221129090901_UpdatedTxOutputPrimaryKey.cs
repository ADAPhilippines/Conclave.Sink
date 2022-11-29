using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTxOutputPrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput",
                columns: new[] { "TxHash", "Index", "BlockHash", "BlockNumber", "Slot", "Epoch" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput",
                columns: new[] { "TxHash", "Index" });
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class FixedBugInTxInputModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.RenameColumn(
                name: "Index",
                table: "TxInput",
                newName: "TxInputOutputIndex");

            migrationBuilder.AddColumn<string>(
                name: "TxInputOutputHash",
                table: "TxInput",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                column: "TxHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxInputOutputHash",
                table: "TxInput");

            migrationBuilder.RenameColumn(
                name: "TxInputOutputIndex",
                table: "TxInput",
                newName: "Index");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                columns: new[] { "TxHash", "Index", "Slot" });
        }
    }
}

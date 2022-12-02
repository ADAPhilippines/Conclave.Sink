using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class TxInputTxOutputRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxInputOutputHash",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxInputOutputIndex",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "TxInput");

            migrationBuilder.AddColumn<decimal>(
                name: "TxOutputIndex",
                table: "TxInput",
                type: "numeric(20,0)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TxOutputTxHash",
                table: "TxInput",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                column: "TxHash");

            migrationBuilder.CreateIndex(
                name: "IX_TxInput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput",
                columns: new[] { "TxOutputTxHash", "TxOutputIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_TxInput_TxOutput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput",
                columns: new[] { "TxOutputTxHash", "TxOutputIndex" },
                principalTable: "TxOutput",
                principalColumns: new[] { "TxHash", "Index" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TxInput_TxOutput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.DropIndex(
                name: "IX_TxInput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxOutputTxHash",
                table: "TxInput");

            migrationBuilder.AddColumn<string>(
                name: "TxInputOutputHash",
                table: "TxInput",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TxInputOutputIndex",
                table: "TxInput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Slot",
                table: "TxInput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                columns: new[] { "TxHash", "TxInputOutputHash", "TxInputOutputIndex", "Slot" });
        }
    }
}

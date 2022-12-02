using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class TxInputTxOuputRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TxInput_Block_BlockHash",
                table: "TxInput");

            migrationBuilder.DropForeignKey(
                name: "FK_TxInput_TxOutput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropForeignKey(
                name: "FK_TxOutput_Block_BlockHash",
                table: "TxOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.DropIndex(
                name: "IX_TxInput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxOutputTxHash",
                table: "TxInput");

            migrationBuilder.AlterColumn<string>(
                name: "BlockHash",
                table: "TxOutput",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "TxOutputIndex",
                table: "TxInput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlockHash",
                table: "TxInput",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TxOutputHash",
                table: "TxInput",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput",
                columns: new[] { "TxHash", "TxOutputHash", "TxOutputIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_TxInput_TxOutputHash_TxOutputIndex",
                table: "TxInput",
                columns: new[] { "TxOutputHash", "TxOutputIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_TxInput_Block_BlockHash",
                table: "TxInput",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TxInput_TxOutput_TxOutputHash_TxOutputIndex",
                table: "TxInput",
                columns: new[] { "TxOutputHash", "TxOutputIndex" },
                principalTable: "TxOutput",
                principalColumns: new[] { "TxHash", "Index" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TxOutput_Block_BlockHash",
                table: "TxOutput",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TxInput_Block_BlockHash",
                table: "TxInput");

            migrationBuilder.DropForeignKey(
                name: "FK_TxInput_TxOutput_TxOutputHash_TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropForeignKey(
                name: "FK_TxOutput_Block_BlockHash",
                table: "TxOutput");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TxInput",
                table: "TxInput");

            migrationBuilder.DropIndex(
                name: "IX_TxInput_TxOutputHash_TxOutputIndex",
                table: "TxInput");

            migrationBuilder.DropColumn(
                name: "TxOutputHash",
                table: "TxInput");

            migrationBuilder.AlterColumn<string>(
                name: "BlockHash",
                table: "TxOutput",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "BlockHash",
                table: "TxInput",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "TxOutputIndex",
                table: "TxInput",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

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
                name: "FK_TxInput_Block_BlockHash",
                table: "TxInput",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_TxInput_TxOutput_TxOutputTxHash_TxOutputIndex",
                table: "TxInput",
                columns: new[] { "TxOutputTxHash", "TxOutputIndex" },
                principalTable: "TxOutput",
                principalColumns: new[] { "TxHash", "Index" });

            migrationBuilder.AddForeignKey(
                name: "FK_TxOutput_Block_BlockHash",
                table: "TxOutput",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash");
        }
    }
}

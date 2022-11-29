using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class AddedBlockModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput");

            migrationBuilder.DropColumn(
                name: "BlockNumber",
                table: "TxOutput");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "TxOutput");

            migrationBuilder.DropColumn(
                name: "Epoch",
                table: "TxOutput");

            migrationBuilder.AlterColumn<string>(
                name: "BlockHash",
                table: "TxOutput",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput",
                columns: new[] { "TxHash", "Index" });

            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    BlockHash = table.Column<string>(type: "text", nullable: false),
                    BlockNumber = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.BlockHash);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TxOutput_BlockHash",
                table: "TxOutput",
                column: "BlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_TxOutput_Block_BlockHash",
                table: "TxOutput",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TxOutput_Block_BlockHash",
                table: "TxOutput");

            migrationBuilder.DropTable(
                name: "Block");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput");

            migrationBuilder.DropIndex(
                name: "IX_TxOutput_BlockHash",
                table: "TxOutput");

            migrationBuilder.AlterColumn<string>(
                name: "BlockHash",
                table: "TxOutput",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BlockNumber",
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

            migrationBuilder.AddColumn<decimal>(
                name: "Epoch",
                table: "TxOutput",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TxOutput",
                table: "TxOutput",
                columns: new[] { "TxHash", "Index", "BlockHash", "BlockNumber", "Slot", "Epoch" });
        }
    }
}

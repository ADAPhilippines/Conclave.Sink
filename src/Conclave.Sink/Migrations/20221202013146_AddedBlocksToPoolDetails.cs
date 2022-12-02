using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class AddedBlocksToPoolDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.RenameColumn(
                name: "Epoch",
                table: "Pools",
                newName: "Slot");

            migrationBuilder.AddColumn<string>(
                name: "BlockHash",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                columns: new[] { "Operator", "Slot", "Ticker" });

            migrationBuilder.CreateIndex(
                name: "IX_Pools_BlockHash",
                table: "Pools",
                column: "BlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_Pools_Block_BlockHash",
                table: "Pools",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pools_Block_BlockHash",
                table: "Pools");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.DropIndex(
                name: "IX_Pools_BlockHash",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "BlockHash",
                table: "Pools");

            migrationBuilder.RenameColumn(
                name: "Slot",
                table: "Pools",
                newName: "Epoch");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                columns: new[] { "Operator", "Epoch" });
        }
    }
}

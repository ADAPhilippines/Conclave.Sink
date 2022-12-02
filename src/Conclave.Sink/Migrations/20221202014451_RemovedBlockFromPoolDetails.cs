using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class RemovedBlockFromPoolDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pools_Block_BlockHash",
                table: "Pools");

            migrationBuilder.DropIndex(
                name: "IX_Pools_BlockHash",
                table: "Pools");

            migrationBuilder.AddColumn<decimal>(
                name: "Epoch",
                table: "Pools",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Epoch",
                table: "Pools");

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
    }
}

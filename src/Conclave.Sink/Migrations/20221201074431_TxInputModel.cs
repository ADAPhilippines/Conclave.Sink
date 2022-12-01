using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class TxInputModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TxInput",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    BlockHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TxInput", x => new { x.TxHash, x.Index });
                    table.ForeignKey(
                        name: "FK_TxInput_Block_BlockHash",
                        column: x => x.BlockHash,
                        principalTable: "Block",
                        principalColumn: "BlockHash");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TxInput_BlockHash",
                table: "TxInput",
                column: "BlockHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TxInput");
        }
    }
}

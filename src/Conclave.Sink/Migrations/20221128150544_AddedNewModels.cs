using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BalanceByAddress",
                columns: table => new
                {
                    Address = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceByAddress", x => x.Address);
                });

            migrationBuilder.CreateTable(
                name: "TxOutput",
                columns: table => new
                {
                    TxHash = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TxOutput", x => new { x.TxHash, x.Index });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceByAddress");

            migrationBuilder.DropTable(
                name: "TxOutput");
        }
    }
}

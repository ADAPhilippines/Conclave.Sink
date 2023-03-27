using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeddySwap.Sink.Data.Migrations.TeddySwapFisoSinkDb
{
    /// <inheritdoc />
    public partial class UpdateFiso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FisoDelegators",
                columns: table => new
                {
                    StakeAddress = table.Column<string>(type: "text", nullable: false),
                    PoolId = table.Column<string>(type: "text", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    StakeAmount = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FisoDelegators", x => new { x.StakeAddress, x.PoolId, x.Epoch });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FisoDelegators");
        }
    }
}

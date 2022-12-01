using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class AddedRewardAddressByPoolIdPerEpoch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RewardAddressByPoolPerEpoch",
                columns: table => new
                {
                    PoolId = table.Column<string>(type: "text", nullable: false),
                    RewardAddress = table.Column<string>(type: "text", nullable: false),
                    Epoch = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardAddressByPoolPerEpoch", x => new { x.PoolId, x.Epoch, x.RewardAddress });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RewardAddressByPoolPerEpoch");
        }
    }
}

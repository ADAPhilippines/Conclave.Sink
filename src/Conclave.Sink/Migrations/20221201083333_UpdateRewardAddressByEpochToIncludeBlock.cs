using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRewardAddressByEpochToIncludeBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Epoch",
                table: "RewardAddressByPoolPerEpoch",
                newName: "Slot");

            migrationBuilder.AddColumn<string>(
                name: "BlockHash",
                table: "RewardAddressByPoolPerEpoch",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RewardAddressByPoolPerEpoch_BlockHash",
                table: "RewardAddressByPoolPerEpoch",
                column: "BlockHash");

            migrationBuilder.AddForeignKey(
                name: "FK_RewardAddressByPoolPerEpoch_Block_BlockHash",
                table: "RewardAddressByPoolPerEpoch",
                column: "BlockHash",
                principalTable: "Block",
                principalColumn: "BlockHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardAddressByPoolPerEpoch_Block_BlockHash",
                table: "RewardAddressByPoolPerEpoch");

            migrationBuilder.DropIndex(
                name: "IX_RewardAddressByPoolPerEpoch_BlockHash",
                table: "RewardAddressByPoolPerEpoch");

            migrationBuilder.DropColumn(
                name: "BlockHash",
                table: "RewardAddressByPoolPerEpoch");

            migrationBuilder.RenameColumn(
                name: "Slot",
                table: "RewardAddressByPoolPerEpoch",
                newName: "Epoch");
        }
    }
}

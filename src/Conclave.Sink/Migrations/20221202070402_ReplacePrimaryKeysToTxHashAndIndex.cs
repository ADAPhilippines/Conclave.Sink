using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePrimaryKeysToTxHashAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RewardAddressByPoolPerEpoch",
                table: "RewardAddressByPoolPerEpoch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DelegatorByEpoch",
                table: "DelegatorByEpoch");

            migrationBuilder.RenameColumn(
                name: "Slot",
                table: "RewardAddressByPoolPerEpoch",
                newName: "TxIndex");

            migrationBuilder.RenameColumn(
                name: "Slot",
                table: "DelegatorByEpoch",
                newName: "TxIndex");

            migrationBuilder.AddColumn<string>(
                name: "TxHash",
                table: "RewardAddressByPoolPerEpoch",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TxHash",
                table: "DelegatorByEpoch",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RewardAddressByPoolPerEpoch",
                table: "RewardAddressByPoolPerEpoch",
                columns: new[] { "PoolId", "RewardAddress", "TxHash", "TxIndex" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DelegatorByEpoch",
                table: "DelegatorByEpoch",
                columns: new[] { "StakeAddress", "PoolHash", "TxHash", "TxIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RewardAddressByPoolPerEpoch",
                table: "RewardAddressByPoolPerEpoch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DelegatorByEpoch",
                table: "DelegatorByEpoch");

            migrationBuilder.DropColumn(
                name: "TxHash",
                table: "RewardAddressByPoolPerEpoch");

            migrationBuilder.DropColumn(
                name: "TxHash",
                table: "DelegatorByEpoch");

            migrationBuilder.RenameColumn(
                name: "TxIndex",
                table: "RewardAddressByPoolPerEpoch",
                newName: "Slot");

            migrationBuilder.RenameColumn(
                name: "TxIndex",
                table: "DelegatorByEpoch",
                newName: "Slot");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RewardAddressByPoolPerEpoch",
                table: "RewardAddressByPoolPerEpoch",
                columns: new[] { "PoolId", "Slot", "RewardAddress" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DelegatorByEpoch",
                table: "DelegatorByEpoch",
                columns: new[] { "StakeAddress", "PoolHash", "Slot" });
        }
    }
}

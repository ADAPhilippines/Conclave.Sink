using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class RevertedMetadataChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Ticker",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Pools");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Pools",
                newName: "PoolMetadata");

            migrationBuilder.RenameColumn(
                name: "HomePage",
                table: "Pools",
                newName: "TxHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                columns: new[] { "Operator", "TxHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.RenameColumn(
                name: "PoolMetadata",
                table: "Pools",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TxHash",
                table: "Pools",
                newName: "HomePage");

            migrationBuilder.AddColumn<decimal>(
                name: "Slot",
                table: "Pools",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Ticker",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                columns: new[] { "Operator", "Slot", "Ticker" });
        }
    }
}

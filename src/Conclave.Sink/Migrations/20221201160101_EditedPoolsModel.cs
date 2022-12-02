using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class EditedPoolsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Epoch",
                table: "Pools",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "HomePage",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Ticker",
                table: "Pools",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Epoch",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "HomePage",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Ticker",
                table: "Pools");
        }
    }
}

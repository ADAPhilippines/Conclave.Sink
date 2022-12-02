using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class EditedPoolModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "MetadataLink",
                table: "Pools");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                columns: new[] { "Operator", "Epoch" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.AddColumn<string>(
                name: "MetadataLink",
                table: "Pools",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                column: "Operator");
        }
    }
}

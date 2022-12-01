using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class editedpoolsdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.AddColumn<List<string>>(
                name: "Relays",
                table: "Pools",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                column: "Operator");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Pools",
                table: "Pools");

            migrationBuilder.DropColumn(
                name: "Relays",
                table: "Pools");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Pools",
                table: "Pools",
                columns: new[] { "Operator", "VRFKeyHash" });
        }
    }
}

using System.Collections.Generic;
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
            migrationBuilder.DropTable(
                name: "PoolRegistrations");

            migrationBuilder.CreateTable(
                name: "Pools",
                columns: table => new
                {
                    Operator = table.Column<string>(type: "text", nullable: false),
                    VRFKeyHash = table.Column<string>(type: "text", nullable: false),
                    Pledge = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Margin = table.Column<decimal>(type: "numeric", nullable: false),
                    RewardAccount = table.Column<string>(type: "text", nullable: false),
                    PoolOwners = table.Column<List<string>>(type: "text[]", nullable: false),
                    PoolMetadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pools", x => new { x.Operator, x.VRFKeyHash });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pools");

            migrationBuilder.CreateTable(
                name: "PoolRegistrations",
                columns: table => new
                {
                    Operator = table.Column<string>(type: "text", nullable: false),
                    VRFKeyHash = table.Column<string>(type: "text", nullable: false),
                    Cost = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Margin = table.Column<decimal>(type: "numeric", nullable: false),
                    Pledge = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PoolMetadata = table.Column<string>(type: "text", nullable: false),
                    PoolOwners = table.Column<List<string>>(type: "text[]", nullable: false),
                    RewardAccount = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolRegistrations", x => new { x.Operator, x.VRFKeyHash });
                });
        }
    }
}

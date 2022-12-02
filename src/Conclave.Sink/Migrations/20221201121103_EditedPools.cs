using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conclave.Sink.Migrations
{
    /// <inheritdoc />
    public partial class EditedPools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PoolMetadata",
                table: "Pools",
                newName: "MetadataLink");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MetadataLink",
                table: "Pools",
                newName: "PoolMetadata");
        }
    }
}

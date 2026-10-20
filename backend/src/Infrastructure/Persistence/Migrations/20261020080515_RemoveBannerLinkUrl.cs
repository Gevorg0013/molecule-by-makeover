using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoleculeByMakeover.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBannerLinkUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "Banners");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "Banners",
                type: "text",
                nullable: true);
        }
    }
}

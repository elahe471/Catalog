using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class catalogItemIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_Name_Slug",
                schema: "catalog",
                table: "CatalogItems",
                columns: new[] { "Name", "Slug" });

            migrationBuilder.CreateIndex(
                name: "UX_CatalogItems_Slug",
                schema: "catalog",
                table: "CatalogItems",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CatalogItems_Name_Slug",
                schema: "catalog",
                table: "CatalogItems");

            migrationBuilder.DropIndex(
                name: "UX_CatalogItems_Slug",
                schema: "catalog",
                table: "CatalogItems");
        }
    }
}

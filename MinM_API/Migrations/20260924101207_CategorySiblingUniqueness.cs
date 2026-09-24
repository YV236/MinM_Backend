using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinM_API.Migrations
{
    /// <inheritdoc />
    public partial class CategorySiblingUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_ParentCategoryId_Name",
                table: "Categories",
                columns: new[] { "Name", "ParentCategoryId" },
                unique: true,
                filter: "\"ParentCategoryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_ParentCategoryId_Slug",
                table: "Categories",
                columns: new[] { "Slug", "ParentCategoryId" },
                unique: true,
                filter: "\"ParentCategoryId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_Root_Name",
                table: "Categories",
                column: "Name",
                unique: true,
                filter: "\"ParentCategoryId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_Categories_Root_Slug",
                table: "Categories",
                column: "Slug",
                unique: true,
                filter: "\"ParentCategoryId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Categories_ParentCategoryId_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "UX_Categories_ParentCategoryId_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "UX_Categories_Root_Name",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "UX_Categories_Root_Slug",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);
        }
    }
}

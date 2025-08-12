using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinM_API.Migrations
{
    /// <inheritdoc />
    public partial class ProductGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<char>(
                name: "SKUGroup",
                table: "Products",
                type: "character(1)",
                nullable: false,
                defaultValue: 'Z');

            migrationBuilder.AddColumn<int>(
                name: "SKUSequence",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SKUGroup",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SKUSequence",
                table: "Products");
        }
    }
}

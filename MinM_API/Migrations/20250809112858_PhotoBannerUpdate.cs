using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinM_API.Migrations
{
    /// <inheritdoc />
    public partial class PhotoBannerUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "URL",
                table: "BannerImages",
                newName: "ImageURL");

            migrationBuilder.AddColumn<string>(
                name: "ButtonText",
                table: "BannerImages",
                type: "character varying(248)",
                maxLength: 248,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PageURL",
                table: "BannerImages",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "BannerImages",
                type: "character varying(248)",
                maxLength: 248,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ButtonText",
                table: "BannerImages");

            migrationBuilder.DropColumn(
                name: "PageURL",
                table: "BannerImages");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "BannerImages");

            migrationBuilder.RenameColumn(
                name: "ImageURL",
                table: "BannerImages",
                newName: "URL");
        }
    }
}

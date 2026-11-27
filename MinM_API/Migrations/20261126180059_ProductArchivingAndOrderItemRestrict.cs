using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinM_API.Migrations
{
    /// <inheritdoc />
    public partial class ProductArchivingAndOrderItemRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_ItemId",
                table: "OrderItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_ItemId",
                table: "OrderItems",
                column: "ItemId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_ItemId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_ItemId",
                table: "OrderItems",
                column: "ItemId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

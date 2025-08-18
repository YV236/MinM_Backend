using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinM_API.Migrations
{
    /// <inheritdoc />
    public partial class UserInfoPropsInOrderRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserFirstName",
                table: "Orders",
                newName: "RecipientFirstName");

            migrationBuilder.RenameColumn(
                name: "UserLastName",
                table: "Orders",
                newName: "RecipientLastName");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "Orders",
                newName: "RecipientEmail");


            migrationBuilder.RenameColumn(
                name: "UserPhone",
                table: "Orders",
                newName: "RecipientPhone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecipientFirstName",
                table: "Orders",
                newName: "UserFirstName");

            migrationBuilder.RenameColumn(
                name: "RecipientLastName",
                table: "Orders",
                newName: "UserLastName");

            migrationBuilder.RenameColumn(
                name: "RecipientEmail",
                table: "Orders",
                newName: "UserEmail");


            migrationBuilder.RenameColumn(
                name: "RecipientPhone",
                table: "Orders",
                newName: "UserPhone");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomm_project_1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSelectINShoppingCartModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "shoppingCarts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "shoppingCarts");
        }
    }
}

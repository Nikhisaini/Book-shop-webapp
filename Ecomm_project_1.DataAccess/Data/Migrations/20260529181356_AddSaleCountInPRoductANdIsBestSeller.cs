using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomm_project_1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCountInPRoductANdIsBestSeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBestseller",
                table: "products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SalesCount",
                table: "products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBestseller",
                table: "products");

            migrationBuilder.DropColumn(
                name: "SalesCount",
                table: "products");
        }
    }
}

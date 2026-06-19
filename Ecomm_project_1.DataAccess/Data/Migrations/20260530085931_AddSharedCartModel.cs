using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomm_project_1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedCartModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedCarts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingCartId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedCarts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedCarts_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SharedCarts_shoppingCarts_ShoppingCartId",
                        column: x => x.ShoppingCartId,
                        principalTable: "shoppingCarts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedCarts_ApplicationUserId",
                table: "SharedCarts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedCarts_ShoppingCartId",
                table: "SharedCarts",
                column: "ShoppingCartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedCarts");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecomm_project_1.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DropPdoductMOdel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    catagoryId = table.Column<int>(type: "int", nullable: false),
                    CoverTypeID = table.Column<int>(type: "int", nullable: false),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListPrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Price100 = table.Column<double>(type: "float", nullable: false),
                    Price50 = table.Column<double>(type: "float", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_Catagories_catagoryId",
                        column: x => x.catagoryId,
                        principalTable: "Catagories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_products_CoverTypes_CoverTypeID",
                        column: x => x.CoverTypeID,
                        principalTable: "CoverTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_catagoryId",
                table: "products",
                column: "catagoryId");

            migrationBuilder.CreateIndex(
                name: "IX_products_CoverTypeID",
                table: "products",
                column: "CoverTypeID");
        }
    }
}

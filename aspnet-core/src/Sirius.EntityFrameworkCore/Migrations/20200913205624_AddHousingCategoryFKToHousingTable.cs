using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddHousingCategoryFKToHousingTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppHousings_HousingCategoryId",
                table: "AppHousings",
                column: "HousingCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousings_AppHousingCategories_HousingCategoryId",
                table: "AppHousings",
                column: "HousingCategoryId",
                principalTable: "AppHousingCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousings_AppHousingCategories_HousingCategoryId",
                table: "AppHousings");

            migrationBuilder.DropIndex(
                name: "IX_AppHousings_HousingCategoryId",
                table: "AppHousings");
        }
    }
}

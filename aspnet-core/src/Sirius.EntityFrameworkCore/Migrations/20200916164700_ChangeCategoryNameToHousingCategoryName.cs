using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class ChangeCategoryNameToHousingCategoryName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "AppHousingCategories");

            migrationBuilder.AddColumn<string>(
                name: "HousingCategoryName",
                table: "AppHousingCategories",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HousingCategoryName",
                table: "AppHousingCategories");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "AppHousingCategories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddIsTenantToHousingPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HousingPersonType",
                table: "AppHousingPeople");

            migrationBuilder.AddColumn<bool>(
                name: "IsTenant",
                table: "AppHousingPeople",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTenant",
                table: "AppHousingPeople");

            migrationBuilder.AddColumn<int>(
                name: "HousingPersonType",
                table: "AppHousingPeople",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

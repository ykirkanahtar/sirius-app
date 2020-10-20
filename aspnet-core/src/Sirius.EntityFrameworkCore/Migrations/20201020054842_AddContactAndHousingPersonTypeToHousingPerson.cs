using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddContactAndHousingPersonTypeToHousingPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Contact",
                table: "AppHousingPeople",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HousingPersonType",
                table: "AppHousingPeople",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contact",
                table: "AppHousingPeople");

            migrationBuilder.DropColumn(
                name: "HousingPersonType",
                table: "AppHousingPeople");
        }
    }
}

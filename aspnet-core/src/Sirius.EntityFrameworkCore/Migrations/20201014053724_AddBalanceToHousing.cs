using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddBalanceToHousing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "AppHousings",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "AppHousings");
        }
    }
}

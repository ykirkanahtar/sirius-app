using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RemovedUnusedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditInAccountBook",
                table: "AppPaymentCategories");

            migrationBuilder.DropColumn(
                name: "ShowInLists",
                table: "AppPaymentCategories");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EditInAccountBook",
                table: "AppPaymentCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInLists",
                table: "AppPaymentCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

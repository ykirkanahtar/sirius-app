using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RenamePeriodForToSiteOrBlock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PeriodFor",
                table: "AppPeriods",
                newName: "SiteOrBlock");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SiteOrBlock",
                table: "AppPeriods",
                newName: "PeriodFor");
        }
    }
}

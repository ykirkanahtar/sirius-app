using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RenameOwnerOrResident : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerOrResident",
                table: "AppHousingPaymentPlanGroups",
                newName: "ResidentOrOwner");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResidentOrOwner",
                table: "AppHousingPaymentPlanGroups",
                newName: "OwnerOrResident");
        }
    }
}

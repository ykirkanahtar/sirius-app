using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddFirstHousingTransferIsResidentOrOwnerToHousingPaymentPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FirstHousingTransferIsResidentOrOwner",
                table: "AppHousingPaymentPlans",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstHousingTransferIsResidentOrOwner",
                table: "AppHousingPaymentPlans");
        }
    }
}

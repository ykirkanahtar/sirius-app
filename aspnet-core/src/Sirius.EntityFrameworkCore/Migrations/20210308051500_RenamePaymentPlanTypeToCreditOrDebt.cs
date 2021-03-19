using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RenamePaymentPlanTypeToCreditOrDebt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentPlanType",
                table: "AppHousingPaymentPlans",
                newName: "CreditOrDebt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreditOrDebt",
                table: "AppHousingPaymentPlans",
                newName: "PaymentPlanType");
        }
    }
}

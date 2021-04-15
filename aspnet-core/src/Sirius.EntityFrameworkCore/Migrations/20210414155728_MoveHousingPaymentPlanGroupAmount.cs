using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class MoveHousingPaymentPlanGroupAmount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPerMonth",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPerMonth",
                table: "AppHousingPaymentPlanGroupHousingCategories",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPerMonth",
                table: "AppHousingPaymentPlanGroupHousingCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPerMonth",
                table: "AppHousingPaymentPlanGroups",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

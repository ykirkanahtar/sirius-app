using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddPaymentCategoryForeignKeyToHousingPaymentPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlans_PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "PaymentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "PaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropIndex(
                name: "IX_AppHousingPaymentPlans_PaymentCategoryId",
                table: "AppHousingPaymentPlans");
        }
    }
}

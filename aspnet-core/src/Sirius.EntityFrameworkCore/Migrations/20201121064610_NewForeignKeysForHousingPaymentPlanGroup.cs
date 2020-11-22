using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class NewForeignKeysForHousingPaymentPlanGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroups_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "HousingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroups_PaymentCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "PaymentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlanGroups_AppHousingCategories_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "HousingCategoryId",
                principalTable: "AppHousingCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlanGroups_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "PaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlanGroups_AppHousingCategories_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlanGroups_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropIndex(
                name: "IX_AppHousingPaymentPlanGroups_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropIndex(
                name: "IX_AppHousingPaymentPlanGroups_PaymentCategoryId",
                table: "AppHousingPaymentPlanGroups");
        }
    }
}

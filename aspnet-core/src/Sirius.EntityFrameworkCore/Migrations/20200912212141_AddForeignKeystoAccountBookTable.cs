using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddForeignKeystoAccountBookTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBookss_FromPaymentAccountId",
                table: "AppAccountBookss",
                column: "FromPaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBookss_HousingId",
                table: "AppAccountBookss",
                column: "HousingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBookss_PaymentCategoryId",
                table: "AppAccountBookss",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBookss_ToPaymentAccountId",
                table: "AppAccountBookss",
                column: "ToPaymentAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBookss_AppPaymentAccounts_FromPaymentAccountId",
                table: "AppAccountBookss",
                column: "FromPaymentAccountId",
                principalTable: "AppPaymentAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBookss_AppHousings_HousingId",
                table: "AppAccountBookss",
                column: "HousingId",
                principalTable: "AppHousings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBookss_AppPaymentCategories_PaymentCategoryId",
                table: "AppAccountBookss",
                column: "PaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBookss_AppPaymentAccounts_ToPaymentAccountId",
                table: "AppAccountBookss",
                column: "ToPaymentAccountId",
                principalTable: "AppPaymentAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBookss_AppPaymentAccounts_FromPaymentAccountId",
                table: "AppAccountBookss");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBookss_AppHousings_HousingId",
                table: "AppAccountBookss");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBookss_AppPaymentCategories_PaymentCategoryId",
                table: "AppAccountBookss");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBookss_AppPaymentAccounts_ToPaymentAccountId",
                table: "AppAccountBookss");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBookss_FromPaymentAccountId",
                table: "AppAccountBookss");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBookss_HousingId",
                table: "AppAccountBookss");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBookss_PaymentCategoryId",
                table: "AppAccountBookss");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBookss_ToPaymentAccountId",
                table: "AppAccountBookss");
        }
    }
}

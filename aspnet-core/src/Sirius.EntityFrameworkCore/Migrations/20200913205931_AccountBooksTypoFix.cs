using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AccountBooksTypoFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppAccountBookss",
                table: "AppAccountBookss");

            migrationBuilder.RenameTable(
                name: "AppAccountBookss",
                newName: "AppAccountBooks");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBookss_ToPaymentAccountId",
                table: "AppAccountBooks",
                newName: "IX_AppAccountBooks_ToPaymentAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBookss_PaymentCategoryId",
                table: "AppAccountBooks",
                newName: "IX_AppAccountBooks_PaymentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBookss_HousingId",
                table: "AppAccountBooks",
                newName: "IX_AppAccountBooks_HousingId");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBookss_FromPaymentAccountId",
                table: "AppAccountBooks",
                newName: "IX_AppAccountBooks_FromPaymentAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppAccountBooks",
                table: "AppAccountBooks",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBooks_AppPaymentAccounts_FromPaymentAccountId",
                table: "AppAccountBooks",
                column: "FromPaymentAccountId",
                principalTable: "AppPaymentAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBooks_AppHousings_HousingId",
                table: "AppAccountBooks",
                column: "HousingId",
                principalTable: "AppHousings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBooks_AppPaymentCategories_PaymentCategoryId",
                table: "AppAccountBooks",
                column: "PaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBooks_AppPaymentAccounts_ToPaymentAccountId",
                table: "AppAccountBooks",
                column: "ToPaymentAccountId",
                principalTable: "AppPaymentAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBooks_AppPaymentAccounts_FromPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBooks_AppHousings_HousingId",
                table: "AppAccountBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBooks_AppPaymentCategories_PaymentCategoryId",
                table: "AppAccountBooks");

            migrationBuilder.DropForeignKey(
                name: "FK_AppAccountBooks_AppPaymentAccounts_ToPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppAccountBooks",
                table: "AppAccountBooks");

            migrationBuilder.RenameTable(
                name: "AppAccountBooks",
                newName: "AppAccountBookss");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBooks_ToPaymentAccountId",
                table: "AppAccountBookss",
                newName: "IX_AppAccountBookss_ToPaymentAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBooks_PaymentCategoryId",
                table: "AppAccountBookss",
                newName: "IX_AppAccountBookss_PaymentCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBooks_HousingId",
                table: "AppAccountBookss",
                newName: "IX_AppAccountBookss_HousingId");

            migrationBuilder.RenameIndex(
                name: "IX_AppAccountBooks_FromPaymentAccountId",
                table: "AppAccountBookss",
                newName: "IX_AppAccountBookss_FromPaymentAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppAccountBookss",
                table: "AppAccountBookss",
                column: "Id");

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
    }
}

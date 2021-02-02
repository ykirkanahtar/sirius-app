using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddAllowNegativeBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowNegativeBalance",
                table: "AppPaymentAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_FromPaymentAccountId",
                table: "AppAccountBooks",
                column: "FromPaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_ToPaymentAccountId",
                table: "AppAccountBooks",
                column: "ToPaymentAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppAccountBooks_AppPaymentAccounts_FromPaymentAccountId",
                table: "AppAccountBooks",
                column: "FromPaymentAccountId",
                principalTable: "AppPaymentAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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
                name: "FK_AppAccountBooks_AppPaymentAccounts_ToPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBooks_FromPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBooks_ToPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.DropColumn(
                name: "AllowNegativeBalance",
                table: "AppPaymentAccounts");
        }
    }
}

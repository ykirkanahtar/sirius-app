using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AccountBookFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBooks_FromPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBooks_HousingId",
                table: "AppAccountBooks");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBooks_PaymentCategoryId",
                table: "AppAccountBooks");

            migrationBuilder.DropIndex(
                name: "IX_AppAccountBooks_ToPaymentAccountId",
                table: "AppAccountBooks");

            migrationBuilder.CreateTable(
                name: "AppAccountBookFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    FileUrl = table.Column<string>(maxLength: 200, nullable: false),
                    AccountBookId = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    DeleterUserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAccountBookFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAccountBookFiles_AppAccountBooks_AccountBookId",
                        column: x => x.AccountBookId,
                        principalTable: "AppAccountBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBookFiles_AccountBookId",
                table: "AppAccountBookFiles",
                column: "AccountBookId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAccountBookFiles");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_FromPaymentAccountId",
                table: "AppAccountBooks",
                column: "FromPaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_HousingId",
                table: "AppAccountBooks",
                column: "HousingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_PaymentCategoryId",
                table: "AppAccountBooks",
                column: "PaymentCategoryId");

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
    }
}

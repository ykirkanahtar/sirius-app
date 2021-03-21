using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class SimplyEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HousingDueType",
                table: "AppPaymentCategories");

            migrationBuilder.AddColumn<int>(
                name: "HousingPaymentPlanType",
                table: "AppHousingPaymentPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TransferFromPaymentCategoryId",
                table: "AppHousingPaymentPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentCategoryId",
                table: "AppAccountBooks",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "TransferPaymentAccount",
                table: "AppAccountBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlans_TransferFromPaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "TransferFromPaymentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_TransferFromPaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "TransferFromPaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_TransferFromPaymentCategoryId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropIndex(
                name: "IX_AppHousingPaymentPlans_TransferFromPaymentCategoryId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropColumn(
                name: "HousingPaymentPlanType",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropColumn(
                name: "TransferFromPaymentCategoryId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropColumn(
                name: "TransferPaymentAccount",
                table: "AppAccountBooks");

            migrationBuilder.AddColumn<int>(
                name: "HousingDueType",
                table: "AppPaymentCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentCategoryId",
                table: "AppAccountBooks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}

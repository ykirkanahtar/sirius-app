using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class NullablePaymentCategoryForHousingPaymentPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "PaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "PaymentCategoryId",
                principalTable: "AppPaymentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

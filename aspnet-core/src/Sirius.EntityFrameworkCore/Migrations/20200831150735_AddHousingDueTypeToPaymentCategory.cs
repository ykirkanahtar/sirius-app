using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddHousingDueTypeToPaymentCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HousingDueType",
                table: "AppPaymentCategories",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HousingDueType",
                table: "AppPaymentCategories");

            migrationBuilder.DropColumn(
                name: "PaymentCategoryId",
                table: "AppHousingPaymentPlans");
        }
    }
}

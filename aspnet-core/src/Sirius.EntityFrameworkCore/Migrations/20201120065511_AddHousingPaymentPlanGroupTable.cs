using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddHousingPaymentPlanGroupTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppHousingPaymentPlanGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    HousingPaymentPlanGroupName = table.Column<string>(nullable: true),
                    HousingCategoryId = table.Column<Guid>(nullable: false),
                    PaymentCategoryId = table.Column<Guid>(nullable: false),
                    AmountPerMonth = table.Column<decimal>(nullable: false),
                    CountOfMonth = table.Column<int>(nullable: false),
                    PaymentDayOfMonth = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingPaymentPlanGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlans_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans",
                column: "HousingPaymentPlanGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlans_AppHousingPaymentPlanGroups_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans",
                column: "HousingPaymentPlanGroupId",
                principalTable: "AppHousingPaymentPlanGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlans_AppHousingPaymentPlanGroups_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropTable(
                name: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropIndex(
                name: "IX_AppHousingPaymentPlans_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans");

            migrationBuilder.DropColumn(
                name: "HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans");
        }
    }
}

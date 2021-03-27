using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class HousingPaymentPlanGroupHousingCategoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousingPaymentPlanGroups_AppHousingCategories_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropIndex(
                name: "IX_AppHousingPaymentPlanGroups_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropColumn(
                name: "HousingCategoryId",
                table: "AppPaymentCategories");

            migrationBuilder.DropColumn(
                name: "HousingCategoryId",
                table: "AppHousingPaymentPlanGroups");

            migrationBuilder.CreateTable(
                name: "AppHousingPaymentPlanGroupHousingCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    HousingCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HousingPaymentPlanGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingPaymentPlanGroupHousingCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHousingPaymentPlanGroupHousingCategories_AppHousingPaymentPlanGroups_HousingPaymentPlanGroupId",
                        column: x => x.HousingPaymentPlanGroupId,
                        principalTable: "AppHousingPaymentPlanGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroupHousingCategories_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlanGroupHousingCategories",
                column: "HousingPaymentPlanGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHousingPaymentPlanGroupHousingCategories");

            migrationBuilder.AddColumn<Guid>(
                name: "HousingCategoryId",
                table: "AppPaymentCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HousingCategoryId",
                table: "AppHousingPaymentPlanGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroups_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "HousingCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousingPaymentPlanGroups_AppHousingCategories_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "HousingCategoryId",
                principalTable: "AppHousingCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

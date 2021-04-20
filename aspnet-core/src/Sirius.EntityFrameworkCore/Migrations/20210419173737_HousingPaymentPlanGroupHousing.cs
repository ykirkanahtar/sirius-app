using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class HousingPaymentPlanGroupHousing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHousingPaymentPlanGroupHousings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    HousingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmountPerMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HousingPaymentPlanGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingPaymentPlanGroupHousings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHousingPaymentPlanGroupHousings_AppHousingPaymentPlanGroups_HousingPaymentPlanGroupId",
                        column: x => x.HousingPaymentPlanGroupId,
                        principalTable: "AppHousingPaymentPlanGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroupHousings_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlanGroupHousings",
                column: "HousingPaymentPlanGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHousingPaymentPlanGroupHousings");
        }
    }
}

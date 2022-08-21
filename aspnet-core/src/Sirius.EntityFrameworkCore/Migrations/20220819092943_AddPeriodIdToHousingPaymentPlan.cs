using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddPeriodIdToHousingPaymentPlan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PeriodId",
                table: "AppHousingPaymentPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppInventories_AccountBookId",
                table: "AppInventories",
                column: "AccountBookId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppInventories_AppAccountBooks_AccountBookId",
                table: "AppInventories",
                column: "AccountBookId",
                principalTable: "AppAccountBooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppInventories_AppAccountBooks_AccountBookId",
                table: "AppInventories");

            migrationBuilder.DropIndex(
                name: "IX_AppInventories_AccountBookId",
                table: "AppInventories");

            migrationBuilder.DropColumn(
                name: "PeriodId",
                table: "AppHousingPaymentPlans");
        }
    }
}

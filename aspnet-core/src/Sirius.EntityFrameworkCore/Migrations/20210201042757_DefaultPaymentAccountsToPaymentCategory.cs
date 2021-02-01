using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class DefaultPaymentAccountsToPaymentCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultFromPaymentAccountId",
                table: "AppPaymentCategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultToPaymentAccountId",
                table: "AppPaymentCategories",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultFromPaymentAccountId",
                table: "AppPaymentCategories");

            migrationBuilder.DropColumn(
                name: "DefaultToPaymentAccountId",
                table: "AppPaymentCategories");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddPaymentCategoryIdToAccountBookEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBookProcessType",
                table: "AppAccountBookss");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentCategoryId",
                table: "AppAccountBookss",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentCategoryId",
                table: "AppAccountBookss");

            migrationBuilder.AddColumn<int>(
                name: "AccountBookProcessType",
                table: "AppAccountBookss",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

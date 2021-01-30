using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class NewFieldsForAccountBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountBookType",
                table: "AppAccountBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EncashmentHousing",
                table: "AppAccountBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "HousingIdForEncachment",
                table: "AppAccountBooks",
                type: "uniqueidentifier",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBookType",
                table: "AppAccountBooks");

            migrationBuilder.DropColumn(
                name: "EncashmentHousing",
                table: "AppAccountBooks");

            migrationBuilder.DropColumn(
                name: "HousingIdForEncachment",
                table: "AppAccountBooks");
        }
    }
}

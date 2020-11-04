using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class BlockFieldForHousingNotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousings_AppBlocks_BlockId",
                table: "AppHousings");

            migrationBuilder.AlterColumn<Guid>(
                name: "BlockId",
                table: "AppHousings",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousings_AppBlocks_BlockId",
                table: "AppHousings",
                column: "BlockId",
                principalTable: "AppBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousings_AppBlocks_BlockId",
                table: "AppHousings");

            migrationBuilder.AlterColumn<Guid>(
                name: "BlockId",
                table: "AppHousings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousings_AppBlocks_BlockId",
                table: "AppHousings",
                column: "BlockId",
                principalTable: "AppBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

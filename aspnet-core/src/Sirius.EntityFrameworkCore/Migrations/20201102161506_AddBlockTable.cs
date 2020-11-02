using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddBlockTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Block",
                table: "AppHousings");

            migrationBuilder.AddColumn<Guid>(
                name: "BlockId",
                table: "AppHousings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppBlocks",
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
                    BlockName = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHousings_BlockId",
                table: "AppHousings",
                column: "BlockId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppHousings_AppBlocks_BlockId",
                table: "AppHousings",
                column: "BlockId",
                principalTable: "AppBlocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppHousings_AppBlocks_BlockId",
                table: "AppHousings");

            migrationBuilder.DropTable(
                name: "AppBlocks");

            migrationBuilder.DropIndex(
                name: "IX_AppHousings_BlockId",
                table: "AppHousings");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "AppHousings");

            migrationBuilder.AddColumn<string>(
                name: "Block",
                table: "AppHousings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}

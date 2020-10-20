using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddHousingPeopleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppHousingPeople",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    HousingId = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingPeople", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHousingPeople_AppHousings_HousingId",
                        column: x => x.HousingId,
                        principalTable: "AppHousings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppHousingPeople_AppPeople_PersonId",
                        column: x => x.PersonId,
                        principalTable: "AppPeople",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPeople_HousingId",
                table: "AppHousingPeople",
                column: "HousingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPeople_PersonId",
                table: "AppHousingPeople",
                column: "PersonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppHousingPeople");
        }
    }
}

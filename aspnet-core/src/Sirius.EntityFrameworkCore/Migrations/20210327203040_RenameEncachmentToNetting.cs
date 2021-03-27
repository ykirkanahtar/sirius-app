using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RenameEncachmentToNetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentCategoryIdForEncachment",
                table: "AppAccountBooks",
                newName: "PaymentCategoryIdForNetting");

            migrationBuilder.RenameColumn(
                name: "HousingIdForEncachment",
                table: "AppAccountBooks",
                newName: "HousingIdForNetting");

            migrationBuilder.RenameColumn(
                name: "EncashmentHousing",
                table: "AppAccountBooks",
                newName: "NettingHousing");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentCategoryIdForNetting",
                table: "AppAccountBooks",
                newName: "PaymentCategoryIdForEncachment");

            migrationBuilder.RenameColumn(
                name: "NettingHousing",
                table: "AppAccountBooks",
                newName: "EncashmentHousing");

            migrationBuilder.RenameColumn(
                name: "HousingIdForNetting",
                table: "AppAccountBooks",
                newName: "HousingIdForEncachment");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RenameFirstHousingDueTransferIsResidentOrOwnerField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstHousingTransferIsResidentOrOwner",
                table: "AppHousingPaymentPlans",
                newName: "FirstHousingDueTransferIsResidentOrOwner");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirstHousingDueTransferIsResidentOrOwner",
                table: "AppHousingPaymentPlans",
                newName: "FirstHousingTransferIsResidentOrOwner");
        }
    }
}

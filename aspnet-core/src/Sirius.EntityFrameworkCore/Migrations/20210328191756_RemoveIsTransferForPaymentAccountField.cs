using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class RemoveIsTransferForPaymentAccountField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransferPaymentAccount",
                table: "AppAccountBooks");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TransferPaymentAccount",
                table: "AppAccountBooks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}

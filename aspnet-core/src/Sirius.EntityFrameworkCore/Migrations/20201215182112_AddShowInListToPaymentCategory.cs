using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddShowInListToPaymentCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "AppPaymentCategories",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInLists",
                table: "AppPaymentCategories",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowInLists",
                table: "AppPaymentCategories");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "AppPaymentCategories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}

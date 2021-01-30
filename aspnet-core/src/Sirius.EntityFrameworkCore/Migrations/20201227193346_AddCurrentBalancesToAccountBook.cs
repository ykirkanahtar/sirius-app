using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AddCurrentBalancesToAccountBook : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FromPaymentAccountCurrentBalance",
                table: "AppAccountBooks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToPaymentAccountCurrentBalance",
                table: "AppAccountBooks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromPaymentAccountCurrentBalance",
                table: "AppAccountBooks");

            migrationBuilder.DropColumn(
                name: "ToPaymentAccountCurrentBalance",
                table: "AppAccountBooks");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sirius.Migrations
{
    public partial class AppInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    BlockName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppBlocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppEmployees",
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
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    Phone1 = table.Column<string>(maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEmployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHousingCategories",
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
                    HousingCategoryName = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPaymentAccounts",
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
                    AccountName = table.Column<string>(maxLength: 50, nullable: false),
                    PaymentAccountType = table.Column<int>(nullable: false),
                    Balance = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    PersonId = table.Column<Guid>(nullable: true),
                    EmployeeId = table.Column<Guid>(nullable: true),
                    Iban = table.Column<string>(nullable: true),
                    TenantIsOwner = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPaymentAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPaymentCategories",
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
                    TenantId = table.Column<int>(nullable: true),
                    PaymentCategoryName = table.Column<string>(maxLength: 50, nullable: true),
                    HousingDueType = table.Column<int>(nullable: true),
                    IsValidForAllPeriods = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPaymentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppPeople",
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
                    FirstName = table.Column<string>(maxLength: 50, nullable: false),
                    LastName = table.Column<string>(maxLength: 50, nullable: false),
                    Phone1 = table.Column<string>(maxLength: 50, nullable: true),
                    Phone2 = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppPeople", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppHousings",
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
                    BlockId = table.Column<Guid>(nullable: false),
                    Apartment = table.Column<string>(maxLength: 50, nullable: true),
                    HousingCategoryId = table.Column<Guid>(nullable: false),
                    Balance = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHousings_AppBlocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "AppBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppHousings_AppHousingCategories_HousingCategoryId",
                        column: x => x.HousingCategoryId,
                        principalTable: "AppHousingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppHousingPaymentPlanGroups",
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
                    HousingPaymentPlanGroupName = table.Column<string>(nullable: true),
                    HousingCategoryId = table.Column<Guid>(nullable: false),
                    PaymentCategoryId = table.Column<Guid>(nullable: false),
                    AmountPerMonth = table.Column<decimal>(nullable: false),
                    CountOfMonth = table.Column<int>(nullable: false),
                    PaymentDayOfMonth = table.Column<int>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingPaymentPlanGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHousingPaymentPlanGroups_AppHousingCategories_HousingCategoryId",
                        column: x => x.HousingCategoryId,
                        principalTable: "AppHousingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppHousingPaymentPlanGroups_AppPaymentCategories_PaymentCategoryId",
                        column: x => x.PaymentCategoryId,
                        principalTable: "AppPaymentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppAccountBooks",
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
                    ProcessDateTime = table.Column<DateTime>(nullable: false),
                    PaymentCategoryId = table.Column<Guid>(nullable: false),
                    HousingId = table.Column<Guid>(nullable: true),
                    FromPaymentAccountId = table.Column<Guid>(nullable: true),
                    ToPaymentAccountId = table.Column<Guid>(nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    DocumentDateTime = table.Column<DateTime>(nullable: true),
                    DocumentNumber = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppAccountBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppAccountBooks_AppPaymentAccounts_FromPaymentAccountId",
                        column: x => x.FromPaymentAccountId,
                        principalTable: "AppPaymentAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppAccountBooks_AppHousings_HousingId",
                        column: x => x.HousingId,
                        principalTable: "AppHousings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppAccountBooks_AppPaymentCategories_PaymentCategoryId",
                        column: x => x.PaymentCategoryId,
                        principalTable: "AppPaymentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppAccountBooks_AppPaymentAccounts_ToPaymentAccountId",
                        column: x => x.ToPaymentAccountId,
                        principalTable: "AppPaymentAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppHousingPeople",
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
                    HousingId = table.Column<Guid>(nullable: false),
                    PersonId = table.Column<Guid>(nullable: false),
                    IsTenant = table.Column<bool>(nullable: false),
                    Contact = table.Column<bool>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "AppHousingPaymentPlans",
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
                    HousingPaymentPlanGroupId = table.Column<Guid>(nullable: true),
                    HousingId = table.Column<Guid>(nullable: false),
                    PaymentCategoryId = table.Column<Guid>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    PaymentPlanType = table.Column<int>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    AccountBookId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppHousingPaymentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppHousingPaymentPlans_AppHousingPaymentPlanGroups_HousingPaymentPlanGroupId",
                        column: x => x.HousingPaymentPlanGroupId,
                        principalTable: "AppHousingPaymentPlanGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppHousingPaymentPlans_AppPaymentCategories_PaymentCategoryId",
                        column: x => x.PaymentCategoryId,
                        principalTable: "AppPaymentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_FromPaymentAccountId",
                table: "AppAccountBooks",
                column: "FromPaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_HousingId",
                table: "AppAccountBooks",
                column: "HousingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_PaymentCategoryId",
                table: "AppAccountBooks",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppAccountBooks_ToPaymentAccountId",
                table: "AppAccountBooks",
                column: "ToPaymentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroups_HousingCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "HousingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlanGroups_PaymentCategoryId",
                table: "AppHousingPaymentPlanGroups",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlans_HousingPaymentPlanGroupId",
                table: "AppHousingPaymentPlans",
                column: "HousingPaymentPlanGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPaymentPlans_PaymentCategoryId",
                table: "AppHousingPaymentPlans",
                column: "PaymentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPeople_HousingId",
                table: "AppHousingPeople",
                column: "HousingId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousingPeople_PersonId",
                table: "AppHousingPeople",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousings_BlockId",
                table: "AppHousings",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_AppHousings_HousingCategoryId",
                table: "AppHousings",
                column: "HousingCategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppAccountBooks");

            migrationBuilder.DropTable(
                name: "AppEmployees");

            migrationBuilder.DropTable(
                name: "AppHousingPaymentPlans");

            migrationBuilder.DropTable(
                name: "AppHousingPeople");

            migrationBuilder.DropTable(
                name: "AppPaymentAccounts");

            migrationBuilder.DropTable(
                name: "AppHousingPaymentPlanGroups");

            migrationBuilder.DropTable(
                name: "AppHousings");

            migrationBuilder.DropTable(
                name: "AppPeople");

            migrationBuilder.DropTable(
                name: "AppPaymentCategories");

            migrationBuilder.DropTable(
                name: "AppBlocks");

            migrationBuilder.DropTable(
                name: "AppHousingCategories");
        }
    }
}

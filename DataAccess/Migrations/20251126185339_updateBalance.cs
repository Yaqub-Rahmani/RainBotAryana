using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SingleRatePolicies_EffectiveFrom_EffectiveTo",
                table: "SingleRatePolicies");

            migrationBuilder.DropIndex(
                name: "IX_OwnerDistributionPolicies_EffectiveFrom_EffectiveTo",
                table: "OwnerDistributionPolicies");

            migrationBuilder.DropIndex(
                name: "IX_FamilySharePolicies_EffectiveFrom_EffectiveTo",
                table: "FamilySharePolicies");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "SingleRatePolicies");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "OwnerDistributionPolicies");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                table: "FamilySharePolicies");

            migrationBuilder.RenameColumn(
                name: "EffectiveFrom",
                table: "SingleRatePolicies",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "EffectiveFrom",
                table: "OwnerDistributionPolicies",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "EffectiveFrom",
                table: "FamilySharePolicies",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "SingleRatePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "SingleRatePolicies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "SingleRatePolicies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ReserveEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "ReserveEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "MoneyEntryId",
                table: "ReserveEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "ReserveEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "ReserveEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ReserveEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "ReserveEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "OwnerDistributionPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "OwnerDistributionPolicies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "OwnerDistributionPolicies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "FranchiseBalanceEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "FranchiseBalanceEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FranchiseBalanceEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MoneyEntryId",
                table: "FranchiseBalanceEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "FranchiseBalanceEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "FranchiseBalanceEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "FranchiseBalanceEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "FranchiseBalanceEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "FamilySharePolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "FamilySharePolicies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "FamilySharePolicies",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "Collections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Collections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Collections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MoneyEntryId",
                table: "Collections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CharityPaymentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoneyEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharityPaymentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharityPaymentEntries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CharityPaymentEntries_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CharityPaymentEntries_OwnerDistributions_OwnerDistributionId",
                        column: x => x.OwnerDistributionId,
                        principalTable: "OwnerDistributions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DollarRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AfnPerUsd = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DollarRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Interest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Taxes = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayDay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MoneyAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxPaymentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoneyEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxPaymentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxPaymentEntries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaxPaymentEntries_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TaxPaymentEntries_OwnerDistributions_OwnerDistributionId",
                        column: x => x.OwnerDistributionId,
                        principalTable: "OwnerDistributions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ZakatPaymentEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MoneyEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZakatPaymentEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZakatPaymentEntries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ZakatPaymentEntries_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ZakatPaymentEntries_OwnerDistributions_OwnerDistributionId",
                        column: x => x.OwnerDistributionId,
                        principalTable: "OwnerDistributions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MoneyEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expense_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MoneyEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MoneyAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    AfnPerUsdAtThatTime = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UsdEquivalent = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyEntries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MoneyEntries_Expense_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expense",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MoneyEntries_MoneyAccounts_MoneyAccountId",
                        column: x => x.MoneyAccountId,
                        principalTable: "MoneyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharityPaymentEntries_CollectionId",
                table: "CharityPaymentEntries",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CharityPaymentEntries_ContractId",
                table: "CharityPaymentEntries",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CharityPaymentEntries_OwnerDistributionId_OccurredAtUtc",
                table: "CharityPaymentEntries",
                columns: new[] { "OwnerDistributionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Expense_EmployeeId",
                table: "Expense",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyEntries_CollectionId",
                table: "MoneyEntries",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyEntries_ExpenseId",
                table: "MoneyEntries",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyEntries_MoneyAccountId",
                table: "MoneyEntries",
                column: "MoneyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxPaymentEntries_CollectionId",
                table: "TaxPaymentEntries",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxPaymentEntries_ContractId",
                table: "TaxPaymentEntries",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxPaymentEntries_OwnerDistributionId_OccurredAtUtc",
                table: "TaxPaymentEntries",
                columns: new[] { "OwnerDistributionId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ZakatPaymentEntries_CollectionId",
                table: "ZakatPaymentEntries",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ZakatPaymentEntries_ContractId",
                table: "ZakatPaymentEntries",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ZakatPaymentEntries_OwnerDistributionId_OccurredAtUtc",
                table: "ZakatPaymentEntries",
                columns: new[] { "OwnerDistributionId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharityPaymentEntries");

            migrationBuilder.DropTable(
                name: "DollarRates");

            migrationBuilder.DropTable(
                name: "MoneyEntries");

            migrationBuilder.DropTable(
                name: "TaxPaymentEntries");

            migrationBuilder.DropTable(
                name: "ZakatPaymentEntries");

            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "MoneyAccounts");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "SingleRatePolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "SingleRatePolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "SingleRatePolicies");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "MoneyEntryId",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "ReserveEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "OwnerDistributionPolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "OwnerDistributionPolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "OwnerDistributionPolicies");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "MoneyEntryId",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "FranchiseBalanceEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "FamilySharePolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "FamilySharePolicies");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "FamilySharePolicies");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "MoneyEntryId",
                table: "Collections");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "SingleRatePolicies",
                newName: "EffectiveFrom");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "OwnerDistributionPolicies",
                newName: "EffectiveFrom");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "FamilySharePolicies",
                newName: "EffectiveFrom");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                table: "SingleRatePolicies",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                table: "OwnerDistributionPolicies",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                table: "FamilySharePolicies",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SingleRatePolicies_EffectiveFrom_EffectiveTo",
                table: "SingleRatePolicies",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_OwnerDistributionPolicies_EffectiveFrom_EffectiveTo",
                table: "OwnerDistributionPolicies",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilySharePolicies_EffectiveFrom_EffectiveTo",
                table: "FamilySharePolicies",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    RelationTag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyPayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerDistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalFamilyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RuleSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilySharePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    FemaleWeight = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    MaleWeight = table.Column<decimal>(type: "decimal(6,3)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilySharePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseOwners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseOwners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MemberType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OwnerDistributionPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RateTax = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateZakat = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateCharity = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateOwnerReserve = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateFamilyPool = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerDistributionPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OwnerDistributions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerAmountInput = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RateTax = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateZakat = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateCharity = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateOwnerReserve = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RateFamilyPool = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AmountTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountZakat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountCharity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountOwnerReserve = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountFamilyPool = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerDistributions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitsInStock = table.Column<short>(type: "smallint", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReserveAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReserveAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SingleRatePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    RatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SingleRatePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TaxOrIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOperationClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OperationClaimId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOperationClaims", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FamilyBalanceEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FamilyPayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerDistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyBalanceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyBalanceEntries_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FamilyPayoutLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyPayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FamilyMemberId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyPayoutLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyPayoutLines_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FamilyPayoutLines_FamilyMembers_FamilyMemberId1",
                        column: x => x.FamilyMemberId1,
                        principalTable: "FamilyMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FamilyPayoutLines_FamilyPayouts_FamilyPayoutId",
                        column: x => x.FamilyPayoutId,
                        principalTable: "FamilyPayouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseBalanceEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FranchiseOwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodYear = table.Column<int>(type: "int", nullable: true),
                    PeriodMonth = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseBalanceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseBalanceEntries_FranchiseOwners_FranchiseOwnerId",
                        column: x => x.FranchiseOwnerId,
                        principalTable: "FranchiseOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Floors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    MarketId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Floors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Floors_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Floors_Markets_MarketId1",
                        column: x => x.MarketId1,
                        principalTable: "Markets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OwnerDistributionLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerDistributionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    RateUsed = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OwnerDistributionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OwnerDistributionLines_OwnerDistributions_OwnerDistributionId",
                        column: x => x.OwnerDistributionId,
                        principalTable: "OwnerDistributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shops_Floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "Floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shops_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerType = table.Column<int>(type: "int", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Street = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BuildingNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SignedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDayOfMonth = table.Column<int>(type: "int", nullable: false),
                    OwnerPortionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FranchisePortionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarketPortionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricityNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeterReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    ReadingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreviousIndex = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CurrentIndex = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ReserveRate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    ShopId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterReadings_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeterReadings_Shops_ShopId1",
                        column: x => x.ShopId1,
                        principalTable: "Shops",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShopFranchiseShares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FranchiseOwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SharePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShopId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FranchiseOwnerId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopFranchiseShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopFranchiseShares_FranchiseOwners_FranchiseOwnerId",
                        column: x => x.FranchiseOwnerId,
                        principalTable: "FranchiseOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopFranchiseShares_FranchiseOwners_FranchiseOwnerId1",
                        column: x => x.FranchiseOwnerId1,
                        principalTable: "FranchiseOwners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShopFranchiseShares_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopFranchiseShares_Shops_ShopId1",
                        column: x => x.ShopId1,
                        principalTable: "Shops",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountReceived = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TargetYear = table.Column<int>(type: "int", nullable: true),
                    TargetMonth = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShopId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collections_Contracts_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Collections_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collections_Shops_ShopId1",
                        column: x => x.ShopId1,
                        principalTable: "Shops",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Collections_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Collections_Tenants_TenantId1",
                        column: x => x.TenantId1,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accruals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeterReadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShopId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MeterReadingId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accruals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accruals_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accruals_Contracts_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accruals_MeterReadings_MeterReadingId",
                        column: x => x.MeterReadingId,
                        principalTable: "MeterReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Accruals_MeterReadings_MeterReadingId1",
                        column: x => x.MeterReadingId1,
                        principalTable: "MeterReadings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accruals_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accruals_Shops_ShopId1",
                        column: x => x.ShopId1,
                        principalTable: "Shops",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Accruals_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accruals_Tenants_TenantId1",
                        column: x => x.TenantId1,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CollectionAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MeterReadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodYear = table.Column<int>(type: "int", nullable: true),
                    PeriodMonth = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CollectionId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ShopId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MeterReadingId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Collections_CollectionId1",
                        column: x => x.CollectionId1,
                        principalTable: "Collections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Contracts_ContractId1",
                        column: x => x.ContractId1,
                        principalTable: "Contracts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_MeterReadings_MeterReadingId",
                        column: x => x.MeterReadingId,
                        principalTable: "MeterReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_MeterReadings_MeterReadingId1",
                        column: x => x.MeterReadingId1,
                        principalTable: "MeterReadings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Shops_ShopId1",
                        column: x => x.ShopId1,
                        principalTable: "Shops",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CollectionAllocations_Tenants_TenantId1",
                        column: x => x.TenantId1,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AccrualSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CollectionAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccrualId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SettledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CollectionAllocationId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccrualId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccrualSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccrualSettlements_Accruals_AccrualId",
                        column: x => x.AccrualId,
                        principalTable: "Accruals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccrualSettlements_Accruals_AccrualId1",
                        column: x => x.AccrualId1,
                        principalTable: "Accruals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccrualSettlements_CollectionAllocations_CollectionAllocationId",
                        column: x => x.CollectionAllocationId,
                        principalTable: "CollectionAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccrualSettlements_CollectionAllocations_CollectionAllocationId1",
                        column: x => x.CollectionAllocationId1,
                        principalTable: "CollectionAllocations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReserveEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReserveAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CollectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CollectionAllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MeterReadingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExternalDocNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReserveEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReserveEntries_CollectionAllocations_CollectionAllocationId",
                        column: x => x.CollectionAllocationId,
                        principalTable: "CollectionAllocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReserveEntries_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReserveEntries_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReserveEntries_MeterReadings_MeterReadingId",
                        column: x => x.MeterReadingId,
                        principalTable: "MeterReadings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReserveEntries_ReserveAccounts_ReserveAccountId",
                        column: x => x.ReserveAccountId,
                        principalTable: "ReserveAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_ContractId",
                table: "Accruals",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_ContractId1",
                table: "Accruals",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_MeterReadingId",
                table: "Accruals",
                column: "MeterReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_MeterReadingId1",
                table: "Accruals",
                column: "MeterReadingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_ShopId_TenantId_ContractId_Kind_Year_Month",
                table: "Accruals",
                columns: new[] { "ShopId", "TenantId", "ContractId", "Kind", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_ShopId1",
                table: "Accruals",
                column: "ShopId1");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_TenantId",
                table: "Accruals",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_TenantId1",
                table: "Accruals",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_AccrualSettlements_AccrualId_CollectionAllocationId",
                table: "AccrualSettlements",
                columns: new[] { "AccrualId", "CollectionAllocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccrualSettlements_AccrualId1",
                table: "AccrualSettlements",
                column: "AccrualId1");

            migrationBuilder.CreateIndex(
                name: "IX_AccrualSettlements_CollectionAllocationId",
                table: "AccrualSettlements",
                column: "CollectionAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccrualSettlements_CollectionAllocationId1",
                table: "AccrualSettlements",
                column: "CollectionAllocationId1");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_DistrictId",
                table: "Addresses",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_OwnerType_OwnerId_IsPrimary",
                table: "Addresses",
                columns: new[] { "OwnerType", "OwnerId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ProvinceId",
                table: "Addresses",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_CollectionId_Kind",
                table: "CollectionAllocations",
                columns: new[] { "CollectionId", "Kind" });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_CollectionId1",
                table: "CollectionAllocations",
                column: "CollectionId1");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_ContractId",
                table: "CollectionAllocations",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_ContractId1",
                table: "CollectionAllocations",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_MeterReadingId",
                table: "CollectionAllocations",
                column: "MeterReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_MeterReadingId1",
                table: "CollectionAllocations",
                column: "MeterReadingId1");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_ShopId",
                table: "CollectionAllocations",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_ShopId1",
                table: "CollectionAllocations",
                column: "ShopId1");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_TenantId",
                table: "CollectionAllocations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionAllocations_TenantId1",
                table: "CollectionAllocations",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ContractId",
                table: "Collections",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ContractId1",
                table: "Collections",
                column: "ContractId1");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ShopId_TenantId_ContractId_PaidAtUtc",
                table: "Collections",
                columns: new[] { "ShopId", "TenantId", "ContractId", "PaidAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Collections_ShopId1",
                table: "Collections",
                column: "ShopId1");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_TenantId",
                table: "Collections",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_TenantId1",
                table: "Collections",
                column: "TenantId1");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ShopId_TenantId_Status_StartDate_EndDate",
                table: "Contracts",
                columns: new[] { "ShopId", "TenantId", "Status", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_TenantId",
                table: "Contracts",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_ProvinceId",
                table: "Districts",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_ProvinceId_Name",
                table: "Districts",
                columns: new[] { "ProvinceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyBalanceEntries_FamilyMemberId_OccurredAtUtc",
                table: "FamilyBalanceEntries",
                columns: new[] { "FamilyMemberId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyPayoutLines_FamilyMemberId",
                table: "FamilyPayoutLines",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyPayoutLines_FamilyMemberId1",
                table: "FamilyPayoutLines",
                column: "FamilyMemberId1");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyPayoutLines_FamilyPayoutId",
                table: "FamilyPayoutLines",
                column: "FamilyPayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilySharePolicies_EffectiveFrom",
                table: "FamilySharePolicies",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Floors_MarketId_Level",
                table: "Floors",
                columns: new[] { "MarketId", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Floors_MarketId1",
                table: "Floors",
                column: "MarketId1");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseBalanceEntries_FranchiseOwnerId_OccurredAtUtc",
                table: "FranchiseBalanceEntries",
                columns: new[] { "FranchiseOwnerId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_ShopId_Year_Month",
                table: "MeterReadings",
                columns: new[] { "ShopId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_ShopId1",
                table: "MeterReadings",
                column: "ShopId1");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerDistributionLines_OwnerDistributionId",
                table: "OwnerDistributionLines",
                column: "OwnerDistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_OwnerDistributionPolicies_EffectiveFrom",
                table: "OwnerDistributionPolicies",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_Name",
                table: "Provinces",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReserveAccounts_Type",
                table: "ReserveAccounts",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReserveEntries_CollectionAllocationId",
                table: "ReserveEntries",
                column: "CollectionAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveEntries_CollectionId",
                table: "ReserveEntries",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveEntries_ContractId",
                table: "ReserveEntries",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveEntries_MeterReadingId",
                table: "ReserveEntries",
                column: "MeterReadingId");

            migrationBuilder.CreateIndex(
                name: "IX_ReserveEntries_ReserveAccountId_OccurredAtUtc",
                table: "ReserveEntries",
                columns: new[] { "ReserveAccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopFranchiseShares_FranchiseOwnerId",
                table: "ShopFranchiseShares",
                column: "FranchiseOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopFranchiseShares_FranchiseOwnerId1",
                table: "ShopFranchiseShares",
                column: "FranchiseOwnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_ShopFranchiseShares_ShopId_FranchiseOwnerId_EffectiveFrom",
                table: "ShopFranchiseShares",
                columns: new[] { "ShopId", "FranchiseOwnerId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopFranchiseShares_ShopId1",
                table: "ShopFranchiseShares",
                column: "ShopId1");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_FloorId_UnitNumber",
                table: "Shops",
                columns: new[] { "FloorId", "UnitNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Shops_MarketId",
                table: "Shops",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_SingleRatePolicies_Type_EffectiveFrom",
                table: "SingleRatePolicies",
                columns: new[] { "Type", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccrualSettlements");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "FamilyBalanceEntries");

            migrationBuilder.DropTable(
                name: "FamilyPayoutLines");

            migrationBuilder.DropTable(
                name: "FamilySharePolicies");

            migrationBuilder.DropTable(
                name: "FranchiseBalanceEntries");

            migrationBuilder.DropTable(
                name: "MarketMembers");

            migrationBuilder.DropTable(
                name: "OperationClaims");

            migrationBuilder.DropTable(
                name: "OwnerDistributionLines");

            migrationBuilder.DropTable(
                name: "OwnerDistributionPolicies");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ReserveEntries");

            migrationBuilder.DropTable(
                name: "ShopFranchiseShares");

            migrationBuilder.DropTable(
                name: "SingleRatePolicies");

            migrationBuilder.DropTable(
                name: "UserOperationClaims");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Accruals");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "FamilyPayouts");

            migrationBuilder.DropTable(
                name: "OwnerDistributions");

            migrationBuilder.DropTable(
                name: "CollectionAllocations");

            migrationBuilder.DropTable(
                name: "ReserveAccounts");

            migrationBuilder.DropTable(
                name: "FranchiseOwners");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropTable(
                name: "MeterReadings");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Shops");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Floors");

            migrationBuilder.DropTable(
                name: "Markets");
        }
    }
}

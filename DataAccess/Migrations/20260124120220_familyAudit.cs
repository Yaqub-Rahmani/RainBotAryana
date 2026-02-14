using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class familyAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "FamilyBalanceEntries",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "FamilyBalanceEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FamilyBalanceEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "FamilyBalanceEntries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "FamilyBalanceEntries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodMonth",
                table: "FamilyBalanceEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeriodYear",
                table: "FamilyBalanceEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "FamilyBalanceEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "FamilyBalanceEntries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "PeriodMonth",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "PeriodYear",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "FamilyBalanceEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "FamilyBalanceEntries");
        }
    }
}

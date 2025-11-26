using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class payBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "MeterReadings",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinalizedAt",
                table: "MeterReadings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinalizedByUserId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "MeterReadings",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "MeterReadings",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArchivedByUserId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Collections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Collections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Collections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CollectionAllocations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "CollectionAllocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SettledByUserId",
                table: "AccrualSettlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Accruals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Accruals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "Accruals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeterReadings_ContractId",
                table: "MeterReadings",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeterReadings_Contracts_ContractId",
                table: "MeterReadings",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeterReadings_Contracts_ContractId",
                table: "MeterReadings");

            migrationBuilder.DropIndex(
                name: "IX_MeterReadings_ContractId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "FinalizedAt",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "FinalizedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Collections");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CollectionAllocations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "CollectionAllocations");

            migrationBuilder.DropColumn(
                name: "SettledByUserId",
                table: "AccrualSettlements");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Accruals");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Accruals");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Accruals");
        }
    }
}

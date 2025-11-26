using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class UserIdUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eski GUID kolonlarını sil
            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "FinalizedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeterReadings");

            // Yeni int kolonları ekle
            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserId",
                table: "MeterReadings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinalizedByUserId",
                table: "MeterReadings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "MeterReadings",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // int kolonları sil
            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "FinalizedByUserId",
                table: "MeterReadings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MeterReadings");

            // Eski GUID kolonlarını geri ekle
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FinalizedByUserId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "MeterReadings",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}

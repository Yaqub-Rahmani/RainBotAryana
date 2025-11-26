using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFranchiseOwnerBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FranchiseOwnerBalances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FranchiseOwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedAtBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIn = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOut = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseOwnerBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseOwnerBalances_FranchiseOwners_FranchiseOwnerId",
                        column: x => x.FranchiseOwnerId,
                        principalTable: "FranchiseOwners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseOwnerBalances_FranchiseOwnerId",
                table: "FranchiseOwnerBalances",
                column: "FranchiseOwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FranchiseOwnerBalances");
        }
    }
}

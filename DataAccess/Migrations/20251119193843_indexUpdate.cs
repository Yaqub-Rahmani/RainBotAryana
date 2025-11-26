using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class indexUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accruals_ShopId_TenantId_ContractId_Year_Month",
                table: "Accruals");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_ShopId_TenantId_ContractId_Year_Month_Kind",
                table: "Accruals",
                columns: new[] { "ShopId", "TenantId", "ContractId", "Year", "Month", "Kind" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accruals_ShopId_TenantId_ContractId_Year_Month_Kind",
                table: "Accruals");

            migrationBuilder.CreateIndex(
                name: "IX_Accruals_ShopId_TenantId_ContractId_Year_Month",
                table: "Accruals",
                columns: new[] { "ShopId", "TenantId", "ContractId", "Year", "Month" },
                unique: true);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Pdf.Contracts
{
    public sealed class ContractDebtPeriodLineModel
    {
        public string Period { get; set; } = "";   // e.g. "09/1404"
        public string Amount { get; set; } = "";   // e.g. "1,300 ؋"
    }

    public sealed class ContractDebtStatusA6Model
    {
        // Header - shop/tenant
        public string MarketName { get; set; } = "";
        public string FloorText { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string BusinessActivity { get; set; } = "";
        public string TenantName { get; set; } = "";

        // Franchise (name list) - ALWAYS show if exists
        public bool HasFranchiseOwners { get; set; }
        public string FranchiseOwnersText { get; set; } = ""; // e.g. "امتیاز: احمد و 2 نفر دیگر"

        // Report time
        public string ReportWeekday { get; set; } = "";
        public string ReportDateBig { get; set; } = "";
        public string ReportTime { get; set; } = "";

        // Totals
        public string OldDebtTotal { get; set; } = "";
        public string CurrentDebtTotal { get; set; } = "";
        public string TotalDebt { get; set; } = "";

        // Old debt detail lines
        public bool ShowOldDebtDetail { get; set; }
        public List<ContractDebtPeriodLineModel> OldDebtLines { get; set; } = new();

        // Current period label
        public string CurrentPeriod { get; set; } = ""; // e.g. "1404/10"

        // Current period breakdown (kind-based)
        public string CurrentOwnerPortion { get; set; } = "";      // Market sahibi payı
        public string CurrentMarketPortion { get; set; } = "";      // Market payı
        public string CurrentFranchisePortion { get; set; } = "";   // İmtiyaz payı

        // Electricity extra
        public string CurrentElectricityKwh { get; set; } = "";     // e.g. "125.5 kWh"
        public string CurrentElectricityAmount { get; set; } = "";  // e.g. "900 ؋"

        // Footer
        public string PreparedByUserName { get; set; } = "";
        public string DisclaimerText { get; set; } = ""; // "این سند بدون امضا و مهر معتبر است."
    }

}

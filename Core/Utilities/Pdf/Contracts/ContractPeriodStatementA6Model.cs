using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Pdf.Contracts
{
    public sealed class ContractPeriodStatementA6LineModel
    {
        public string RowNo { get; set; } = "";     // "1"
        public string Period { get; set; } = "";    // "1404/09"
        public string Kwh { get; set; } = "";       // "30.0 kWh"
        public string TotalDebt { get; set; } = ""; // "13,000 ؋"
        public string Paid { get; set; } = "";      // "10,000 ؋"
        public string Remaining { get; set; } = ""; // "3,000 ؋"
    }

    public sealed class ContractPeriodStatementA6Model
    {
        // Header (same family as previous templates)
        public string MarketName { get; set; } = "";
        public string FloorText { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string TenantName { get; set; } = "";
        public string BusinessActivity { get; set; } = "";

        public string FranchiseOwnerText { get; set; } = "";
        // Upper header date/time
        public string ReportWeekday { get; set; } = "";
        public string ReportDateBig { get; set; } = "";
        public string ReportTime { get; set; } = "";

        // Subheader
        public string DebtStatusText { get; set; } = ""; // e.g. "وضعیت بدهی: 24,500 ؋"

        // Range label
        public string RangeText { get; set; } = ""; // e.g. "از 1404/08 تا 1404/12"

        // Table
        public List<ContractPeriodStatementA6LineModel> Lines { get; set; } = new();

        // Footer
        public string PreparedByUserName { get; set; } = "";
        public string StampInfoText { get; set; } = ""; // "بدون مهر/امضا..." veya senin istediğin metin
    }

}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.tenant
{
    public class TenantPeriodSummaryDto : IDto
    {
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public int Year { get; set; }   // Accrual.Year / Collection.TargetYear (null => 0)
        public int Month { get; set; }  // Accrual.Month / Collection.TargetMonth (null => 0)

        public string? MarketName { get; set; }
        public string? FloorName { get; set; }
        public string? ShopName { get; set; }
        public string? UnitNumber { get; set; }

        public decimal DebtAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}

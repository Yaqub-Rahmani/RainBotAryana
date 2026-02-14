using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract
{
    public class AccrualHistoryItemDto
    {
        public int Year { get; set; }
        public int Month { get; set; }

        public decimal OwnerAmount { get; set; }
        public decimal FranchiseAmount { get; set; }
        public decimal MarketAmount { get; set; }
        public decimal ElectricityAmount { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}

using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Accrual
{
    public class AccrualOpenItemDto
    {
        public Guid AccrualId { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public AllocationKind Kind { get; set; }
        public string KindDisplayName { get; set; } = null!;

        public decimal Amount { get; set; }
        public decimal OutstandingAmount { get; set; }

        public string PeriodLabel => $"{Year}/{Month:00}";
    }
}

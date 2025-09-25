using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class AccrualSettlementDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid CollectionAllocationId { get; set; }
        public Guid AccrualId { get; set; }
        public decimal Amount { get; set; }
        public DateTime SettledAtUtc { get; set; }
        public string? Notes { get; set; }

        public RefDto? Allocation { get; set; } = null!;
        public RefDto? Accrual { get; set; } = null!;
    }

}

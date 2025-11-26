using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.AccrualSettlement
{
    public class AccrualSettlementCreateDto: IDto
    {
        public Guid CollectionAllocationId { get; set; }
        public Guid AccrualId { get; set; }
        public decimal Amount { get; set; }        // > 0
        public string? Notes { get; set; }
    }
}

using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract
{
    public class CollectionHistoryItemDto
    {
        public Guid CollectionId { get; set; }

        public DateOnly PaidAtUtc { get; set; }
        public decimal AmountReceived { get; set; }
        public PaymentMethod Method { get; set; }

        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }

        // Tahsilat içi kırılım (AllocationKind)
        public decimal OwnerAllocated { get; set; }
        public decimal MarketAllocated { get; set; }
        public decimal FranchiseAllocated { get; set; }
        public decimal ElectricityAllocated { get; set; }
    }
}

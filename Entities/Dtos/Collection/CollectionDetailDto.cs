using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Collection
{
    public class CollectionDetailDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public DateTime PaidAtUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public decimal AmountReceived { get; set; }
        public PaymentMethod Method { get; set; }
        public string? ReferenceNo { get; set; }
        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
        public string? Notes { get; set; }

        // Zengin alanlar:
        public int AllocationCount { get; set; }
        public decimal AllocatedTotal { get; set; }
        public decimal UnallocatedAmount { get; set; }
    }
}

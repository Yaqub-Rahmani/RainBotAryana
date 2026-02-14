using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.CollectionAllocation
{
    public class CollectionAllocationDto
    {
        public Guid Id { get; set; }
        public Guid CollectionId { get; set; }
        public AllocationKind Kind { get; set; }

        public decimal Amount { get; set; }              // AFN
        public decimal AfnPerUsdAtThatTime { get; set; } // Kullanılan kur
        public decimal UsdEquivalent { get; set; }       // Amount / AfnPerUsd

        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? MeterReadingId { get; set; }

        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }

        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class CollectionAllocationCreateDto
    {
        public Guid CollectionId { get; set; }
        public AllocationKind Kind { get; set; }
        public decimal Amount { get; set; }

        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? MeterReadingId { get; set; }

        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }

        public string? Notes { get; set; }
    }
}

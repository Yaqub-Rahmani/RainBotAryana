using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CollectionAllocationDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid CollectionId { get; set; }

        public string Kind { get; set; } = "";
        public decimal Amount { get; set; }

        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? MeterReadingId { get; set; }

        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }
        public string? Notes { get; set; }

        // ek bilgi
        public string? ShopName { get; set; }
        public string? TenantName { get; set; }
    }
}

using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.Collection
{
    public class CollectionCreateDto:IDto
    {
        public Guid TenantId { get; set; }
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
        public decimal AmountReceived { get; set; }
        public PaymentMethod Method { get; set; }
        public string? ReferenceNo { get; set; }

        public int? TargetYear { get; set; }   // 1300–1500 (opsiyonel)
        public int? TargetMonth { get; set; }  // 1–12 (opsiyonel)
        public string? Notes { get; set; }
    }
}

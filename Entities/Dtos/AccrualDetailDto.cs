using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Dtos
{
    public class AccrualDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public Guid ContractId { get; set; }
        public Guid? MeterReadingId { get; set; }
        public string Kind { get; set; } = null!;    // "Owner" vs
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string Status { get; set; } = null!;  // "Open" vs
        public decimal Amount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string? Notes { get; set; }

        // Özet ilişkiler
        public RefDto Shop { get; set; } = null!;
        public RefDto Tenant { get; set; } = null!;
        public RefDto Contract { get; set; } = null!;
        public RefDto? MeterReading { get; set; }

        // Türetilen alanlar
        public string Period => $"{Year:D4}-{Month:D2}";
        public bool IsSettled => OutstandingAmount == 0m;
    }

}

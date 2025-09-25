using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ContractDetailDto : IDto
    {
        public Guid Id { get; set; }

        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = "";

        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = "";

        public DateOnly SignedOn { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DueDayOfMonth { get; set; }

        public decimal OwnerPortionAmount { get; set; }
        public decimal FranchisePortionAmount { get; set; }
        public decimal MarketPortionAmount { get; set; }
        public decimal TotalFixedRent { get; set; }

        public string? ElectricityNote { get; set; }
        public string Status { get; set; } = "";
        public string? Version { get; set; }
        public string? ArchiveReason { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        // Türetilen kısa özet
        public string Period => $"{StartDate:yyyy-MM-dd} → {EndDate:yyyy-MM-dd}";
        public bool IsActive => Status == "Active";
    }
}

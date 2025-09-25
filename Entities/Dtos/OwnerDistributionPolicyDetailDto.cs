using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OwnerDistributionPolicyDetailDto : IDto
    {
        public Guid Id { get; set; }
        public decimal RateTax { get; set; }
        public decimal RateZakat { get; set; }
        public decimal RateCharity { get; set; }
        public decimal RateOwnerReserve { get; set; }
        public decimal RateFamilyPool { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public string? Notes { get; set; }

        public decimal Sum => RateTax + RateZakat + RateCharity + RateOwnerReserve + RateFamilyPool;
        public string Period => $"{EffectiveFrom:yyyy-MM-dd} → {(EffectiveTo.HasValue ? EffectiveTo.Value.ToString("yyyy-MM-dd") : "∞")}";
    }
}

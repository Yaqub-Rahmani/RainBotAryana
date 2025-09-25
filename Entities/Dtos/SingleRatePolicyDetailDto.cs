using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class SingleRatePolicyDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = "";
        public decimal RatePercent { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public string? Notes { get; set; }
        public string Period => $"{EffectiveFrom:yyyy-MM-dd} → {(EffectiveTo.HasValue ? EffectiveTo.Value.ToString("yyyy-MM-dd") : "∞")}";
    }
}

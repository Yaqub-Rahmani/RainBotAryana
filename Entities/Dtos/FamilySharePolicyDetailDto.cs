using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FamilySharePolicyDetailDto : IDto
    {
        public Guid Id { get; set; }
        public decimal ParentPercent { get; set; }
        public decimal FemaleWeight { get; set; }
        public decimal MaleWeight { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public string? Notes { get; set; }

        public string Period =>
            $"{EffectiveFrom:yyyy-MM-dd} → {EffectiveTo?.ToString("yyyy-MM-dd") ?? "∞"}";
        public bool IsOpenEnded => !EffectiveTo.HasValue;
    }
}

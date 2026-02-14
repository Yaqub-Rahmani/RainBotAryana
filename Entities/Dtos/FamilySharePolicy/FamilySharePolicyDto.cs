using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FamilySharePolicy
{
    public class FamilySharePolicyDto
    {
        public Guid Id { get; set; }

        public decimal ParentPercent { get; set; }
        public decimal FemaleWeight { get; set; }
        public decimal MaleWeight { get; set; }

        public string? Notes { get; set; }

        public DateOnly CreatedAtUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    public class FamilySharePolicyCreateDto
    {
        public decimal ParentPercent { get; set; } = 8.00m;
        public decimal FemaleWeight { get; set; } = 1.000m;
        public decimal MaleWeight { get; set; } = 2.000m;

        public string? Notes { get; set; }

        /// <summary>Boş gelirse bugünün (UTC) tarihi kullanılacak.</summary>
        public DateOnly? CreatedAtUtc { get; set; }
    }

    public class FamilySharePolicyUpdateDto
    {
        public Guid Id { get; set; }

        public decimal ParentPercent { get; set; }
        public decimal FemaleWeight { get; set; }
        public decimal MaleWeight { get; set; }

        public string? Notes { get; set; }
    }
}

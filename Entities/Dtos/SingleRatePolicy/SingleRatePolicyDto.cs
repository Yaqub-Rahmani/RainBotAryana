using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.SingleRatePolicy
{
    public class SingleRatePolicyDto
    {
        public Guid Id { get; set; }
        public SingleRateType Type { get; set; }
        public decimal RatePercent { get; set; }
        public DateOnly CreatedAtUtc { get; set; }
        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public string? Notes { get; set; }
    }

    public class SingleRatePolicyCreateDto
    {
        public SingleRateType Type { get; set; }
        public decimal RatePercent { get; set; }

        /// <summary>
        /// Boş gelirse bugünün tarihi (UTC) kullanılacak.
        /// </summary>
        public DateOnly? CreatedAtUtc { get; set; }

        public string? Notes { get; set; }
    }

    public class SingleRatePolicyUpdateDto
    {
        public Guid Id { get; set; }
        public decimal RatePercent { get; set; }
        public string? Notes { get; set; }
    }
}

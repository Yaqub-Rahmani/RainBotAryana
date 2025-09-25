using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OwnerDistributionLineDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid OwnerDistributionId { get; set; }
        public string Part { get; set; } = ""; // "Tax" vb.
        public decimal RateUsed { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        // Header özeti (isteğe bağlı)
        public decimal? OwnerAmountInput { get; set; }
        public DateTime? OccurredAtUtc { get; set; }
    }
}

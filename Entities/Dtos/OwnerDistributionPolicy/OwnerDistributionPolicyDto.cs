using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.OwnerDistributionPolicy
{
    public class OwnerDistributionPolicyDto
    {
        public Guid Id { get; set; }

        public decimal RateTax { get; set; }
        public decimal RateZakat { get; set; }
        public decimal RateCharity { get; set; }
        public decimal RateOwnerReserve { get; set; }
        public decimal RateFamilyPool { get; set; }
        public bool IsActive { get; set; }
        public DateOnly CreatedAtUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public string? Notes { get; set; }
    }

    public class OwnerDistributionPolicyCreateDto
    {
        public decimal RateTax { get; set; } = 20.00m;
        public decimal RateZakat { get; set; } = 2.50m;
        public decimal RateCharity { get; set; } = 1.50m;
        public decimal RateOwnerReserve { get; set; } = 16.00m;
        public decimal RateFamilyPool { get; set; } = 60.00m;

        /// <summary>Boş gelirse bugünün tarihi (UTC) kullanılacak.</summary>
        public DateOnly? CreatedAtUtc { get; set; }

        public string? Notes { get; set; }
    }

    public class OwnerDistributionPolicyUpdateDto
    {
        public Guid Id { get; set; }

        public decimal RateTax { get; set; }
        public decimal RateZakat { get; set; }
        public decimal RateCharity { get; set; }
        public decimal RateOwnerReserve { get; set; }
        public decimal RateFamilyPool { get; set; }

        public string? Notes { get; set; }
    }

}

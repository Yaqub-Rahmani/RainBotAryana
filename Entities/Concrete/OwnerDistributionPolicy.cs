using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete
{

    public class OwnerDistributionPolicy: IEntity
    {
        [Key] public Guid Id { get; set; }

        [Column(TypeName = "decimal(5,2)")] public decimal RateTax { get; set; } = 20.00m;

        [Column(TypeName = "decimal(5,2)")] public decimal RateZakat { get; set; } = 2.50m;

        [Column(TypeName = "decimal(5,2)")] public decimal RateCharity { get; set; } = 1.50m;

        [Column(TypeName = "decimal(5,2)")] public decimal RateOwnerReserve { get; set; } = 16.00m;

        [Column(TypeName = "decimal(5,2)")] public decimal RateFamilyPool { get; set; } = 60.00m;

        [Required]
        public DateOnly CreatedAtUtc { get; set; }

        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public bool IsActive { get; set; }

        [MaxLength(300)] public string? Notes { get; set; }

        public void ValidateOrThrow()
        {
            var sum = RateTax + RateZakat + RateCharity + RateOwnerReserve + RateFamilyPool;
            if (Math.Round(sum, 2) != 100.00m)
                throw new InvalidOperationException("Owner dağıtım oranlarının toplamı %100 olmalıdır.");
        }
    }

}

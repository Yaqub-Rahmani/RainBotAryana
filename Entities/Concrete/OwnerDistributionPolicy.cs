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
    //Market Sahibi İç Dağıtım Oranları(çok alanlı politika)
    public class OwnerDistributionPolicy: IEntity
    {
        [Key] public Guid Id { get; set; }

        // % alanları (ör. 20.00, 2.50, 1.50, 16.00, 60.00)
        [Column(TypeName = "decimal(5,2)")] public decimal RateTax { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateZakat { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateCharity { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateOwnerReserve { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateFamilyPool { get; set; }

        [Required] public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        [MaxLength(300)] public string? Notes { get; set; }

        public void ValidateOrThrow()
        {
            var sum = RateTax + RateZakat + RateCharity + RateOwnerReserve + RateFamilyPool;
            if (Math.Round(sum, 2) != 100.00m)
                throw new InvalidOperationException("Owner dağıtım oranlarının toplamı %100 olmalıdır.");
        }
    }

}

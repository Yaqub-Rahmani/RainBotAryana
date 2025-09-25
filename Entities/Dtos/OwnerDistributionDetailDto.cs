using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OwnerDistributionDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid? ContractId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? CollectionAllocationId { get; set; }

        public decimal OwnerAmountInput { get; set; }

        // Oranlar
        public decimal RateTax { get; set; }
        public decimal RateZakat { get; set; }
        public decimal RateCharity { get; set; }
        public decimal RateOwnerReserve { get; set; }
        public decimal RateFamilyPool { get; set; }

        // Çıkan tutarlar
        public decimal AmountTax { get; set; }
        public decimal AmountZakat { get; set; }
        public decimal AmountCharity { get; set; }
        public decimal AmountOwnerReserve { get; set; }
        public decimal AmountFamilyPool { get; set; }

        public DateTime OccurredAtUtc { get; set; }
        public string? Notes { get; set; }
    }
}

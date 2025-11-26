using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract
{
    public class ContractAddDto
    {
        [Required]
        public Guid ShopId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public DateOnly SignedOn { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Range(1, 31)]
        public int DueDayOfMonth { get; set; }

        public decimal OwnerPortionAmount { get; set; }
        public decimal FranchisePortionAmount { get; set; }
        public decimal MarketPortionAmount { get; set; }

        [MaxLength(500)]
        public string? ElectricityNote { get; set; }

        [MaxLength(500)]
        public string? BusinessActivity { get; set; }

        public string? Version { get; set; }
    }
}

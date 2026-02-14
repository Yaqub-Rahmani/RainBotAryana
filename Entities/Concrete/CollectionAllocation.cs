using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{

        public enum AllocationKind
        {
            Electricity = 1,
            Market = 2, 
            Owner = 3, 
            Franchise = 4
        }

        public class CollectionAllocation : IEntity
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            public Guid CollectionId { get; set; }

            [Required]
            public AllocationKind Kind { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal Amount { get; set; }

            public Guid? ContractId { get; set; }
            public Guid? ShopId { get; set; }
            public Guid? TenantId { get; set; }

            public Guid? MeterReadingId { get; set; }

            public int CreatedByUserId { get; set; }
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;


            [Range(1300, 1500)]
            public int? PeriodYear { get; set; }
            [Range(1, 12)]
            public int? PeriodMonth { get; set; }  

            [MaxLength(300)]
            public string? Notes { get; set; }

            // Navigation
            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(CollectionId))]
            public Collection Collection { get; set; } = null!;

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(ContractId))]
            public Contract? Contract { get; set; }

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(ShopId))]
            public Shop? Shop { get; set; }

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(TenantId))]
            public Tenant? Tenant { get; set; }

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(MeterReadingId))]
            public MeterReading? MeterReading { get; set; }

            [JsonIgnore]
            [ValidateNever]
            public ICollection<AccrualSettlement> AccrualSettlements { get; set; } = new List<AccrualSettlement>();

            [JsonIgnore]
            [ValidateNever]
            public ICollection<FranchiseBalanceEntry> FranchiseBalanceEntries { get; set; } = new List<FranchiseBalanceEntry>();

            [JsonIgnore]
            [ValidateNever]
            public ICollection<OwnerDistribution> OwnerDistributions { get; set; } = new List<OwnerDistribution>();

            [JsonIgnore]
            [ValidateNever]
            public ICollection<ReserveEntry> ReserveEntries { get; set; } = new List<ReserveEntry>();
        }
}

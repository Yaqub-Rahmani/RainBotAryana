using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{

     public class FamilyBalanceEntry : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FamilyMemberId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        // İz bağlantıları (opsiyonel)
        public Guid? FamilyPayoutId { get; set; } 
        public Guid? OwnerDistributionId { get; set; } 
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }


        // ✅ Audit (Franchise gibi)
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public bool IsActive { get; set; } = true;

        // ✅ Payment metadata (Franchise gibi)
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        // ✅ Period (rapor / filtre)
        [Range(1300, 1500)]
        public int? PeriodYear { get; set; }

        [Range(1, 12)]
        public int? PeriodMonth { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FamilyMemberId))]
        public FamilyMember FamilyMember { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FamilyPayoutId))]
        public FamilyPayout? FamilyPayout { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(OwnerDistributionId))]
        public OwnerDistribution? OwnerDistribution { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CollectionId))]
        public Collection? Collection { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }
    }
}

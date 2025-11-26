using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public enum BalanceDirection { In = 1, Out = 2 } // Bakiye hareketi yönü

    public class FranchiseBalanceEntry : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FranchiseOwnerId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; } // In / Out

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // In için +, Out için + (yön belirler)
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        // İz bağlantıları (opsiyonel, rapor kolaylığı)
        public Guid? CollectionId { get; set; }            // Tahsilat
        public Guid? CollectionAllocationId { get; set; }  // Kind=Franchise satırı
        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }

        [Range(1300, 1500)]
        public int? PeriodYear { get; set; }

        [Range(1, 12)]
        public int? PeriodMonth { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        [MaxLength(100)]
        public string? PaymentReference { get; set; }

        public Guid? MoneyEntryId { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(FranchiseOwnerId))]
        public FranchiseOwner FranchiseOwner { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CollectionId))]
        public Collection? Collection { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CollectionAllocationId))]
        public CollectionAllocation? CollectionAllocation { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop? Shop { get; set; }
    }

}

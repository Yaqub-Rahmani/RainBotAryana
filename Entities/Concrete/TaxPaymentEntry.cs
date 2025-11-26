using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public class TaxPaymentEntry : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; } = BalanceDirection.Out;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(300)]
        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        [MaxLength(100)]
        public string? PaymentReference { get; set; }
        public Guid? MoneyEntryId { get; set; }

        // Audit
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        // Bağlantılar
        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }

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

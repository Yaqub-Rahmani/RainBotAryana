using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{

    public class ReserveEntry : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ReserveAccountId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public Guid? MoneyEntryId { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Açıklama</summary>
        [MaxLength(500)]
        public string? Description { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unknown;

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; }
        public Guid? CollectionId { get; set; }          
        public Guid? CollectionAllocationId { get; set; } 
        public Guid? MeterReadingId { get; set; }         
        public Guid? ContractId { get; set; }            

        [MaxLength(100)]
        public string? ExternalDocNo { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ReserveAccountId))]
        public ReserveAccount ReserveAccount { get; set; } = null!;

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
        [ForeignKey(nameof(MeterReadingId))]
        public MeterReading? MeterReading { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }
    }
}

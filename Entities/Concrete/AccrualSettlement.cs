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

    public class AccrualSettlement : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid CollectionAllocationId { get; set; }

        [Required]
        public Guid AccrualId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int SettledByUserId { get; set; }

        public DateTime SettledAtUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(300)]
        public string? Notes { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(CollectionAllocationId))]
        public CollectionAllocation CollectionAllocation { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(AccrualId))]
        public Accrual Accrual { get; set; } = null!;
    }

}

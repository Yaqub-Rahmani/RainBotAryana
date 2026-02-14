using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public enum DistributionPart
    {
        Tax = 1,         // Maliye
        Zakat = 2,       // Zekat
        Charity = 3,     // Sadaka
        OwnerReserve = 4,// Market Sahibi Rezervesi
        FamilyPool = 5   // Aile havuzu
    }

    public class OwnerDistributionLine : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OwnerDistributionId { get; set; }

        [Required]
        public DistributionPart Part { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RateUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(OwnerDistributionId))]
        public OwnerDistribution OwnerDistribution { get; set; } = null!;
    }
}

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


    public class FamilyPayout : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OwnerDistributionId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalFamilyAmount { get; set; }

        [MaxLength(500)]
        public string? RuleSummary { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(OwnerDistributionId))]
        public OwnerDistribution OwnerDistribution { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FamilyPayoutLine> FamilyPayoutLines { get; set; } = new List<FamilyPayoutLine>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FamilyBalanceEntry> FamilyBalanceEntries { get; set; } = new List<FamilyBalanceEntry>();
    }
}

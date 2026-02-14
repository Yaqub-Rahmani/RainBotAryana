using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{
    public enum AccrualStatus
    {
        Open = 1,
        PartiallySettled = 2,
        Settled = 3
    }

    public class Accrual : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ShopId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        [Required]
        public Guid ContractId { get; set; }

        public Guid? MeterReadingId { get; set; }

        [Required]
        public AllocationKind Kind { get; set; }

        [Range(1300, 1500)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public AccrualStatus Status { get; set; } = AccrualStatus.Open;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingAmount { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop Shop { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(TenantId))]
        public Tenant Tenant { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(MeterReadingId))]
        public MeterReading? MeterReading { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<AccrualSettlement> AccrualSettlements { get; set; } = new List<AccrualSettlement>();

        /// <summary>Basit tutarlılık yardımcıları</summary>
        public void InitializeOutstanding() => OutstandingAmount = Amount;

        public void ApplySettlement(decimal amount)
        {
            if (amount < 0) throw new InvalidOperationException("Settlement amount negative olamaz.");
            OutstandingAmount = Math.Round(Math.Max(0m, OutstandingAmount - amount), 2, MidpointRounding.AwayFromZero);
            Status = OutstandingAmount == 0 ? AccrualStatus.Settled : AccrualStatus.PartiallySettled;
        }
    }

}

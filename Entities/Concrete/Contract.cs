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
using Microsoft.EntityFrameworkCore;

namespace Entities.Concrete
{
    public enum ContractStatus
    {
        Active = 1,
        Archived = 2
    }
    public class Contract : IEntity
    {

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ShopId { get; set; }

        [Required]
        public Guid TenantId { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public int? ArchivedByUserId { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }

        [Required]
        public DateOnly SignedOn { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
        [Required]
        public DateOnly EndDate { get; set; }

        [Range(1, 31)]
        public int DueDayOfMonth { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OwnerPortionAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal FranchisePortionAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MarketPortionAmount { get; set; }

        [MaxLength(500)]
        public string? ElectricityNote { get; set; }

        [Precision(18, 3)]
        public decimal? InitialElectricityIndex { get; set; }

        [MaxLength(500)]
        public string? BusinessActivity { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;


        [MaxLength(20)]
        public string? Version { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }

        [MaxLength(500)]
        public string? ArchiveReason { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop Shop { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(TenantId))]
        public Tenant Tenant { get; set; } = null!;

        [NotMapped]
        public decimal TotalFixedRent => OwnerPortionAmount + FranchisePortionAmount + MarketPortionAmount;

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Accrual> Accruals { get; set; } = new List<Accrual>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<Collection> Collections { get; set; } = new List<Collection>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CollectionAllocation> Allocations { get; set; } = new List<CollectionAllocation>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FamilyBalanceEntry> FamilyBalanceEntries { get; set; } = new List<FamilyBalanceEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FranchiseBalanceEntry> FranchiseBalanceEntries { get; set; } = new List<FranchiseBalanceEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<OwnerDistribution> OwnerDistributions { get; set; } = new List<OwnerDistribution>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<ReserveEntry> ReserveEntries { get; set; } = new List<ReserveEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<TaxPaymentEntry> TaxPayments { get; set; } = new List<TaxPaymentEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<ZakatPaymentEntry> ZakatPayments { get; set; } = new List<ZakatPaymentEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<CharityPaymentEntry> CharityPayments { get; set; } = new List<CharityPaymentEntry>();

    }
}

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
    //    Collection(Tahsilat)
    //Bir kiracıdan alınan tek bir ödemeyi temsil eder(tam/kısmi).
    //Hangi dükkan/sözleşme/dönem için olduğuna dair iz bırakmayı kolaylaştırır.
    //Dağıtım detayları CollectionAllocation satırlarında tutulur(business öncelik sırasına göre).

    public enum PaymentMethod
    {
        //Cash = 1,        // Nakit
        //BankTransfer = 2,// Havale/EFT
        //Pos = 3          // POS/kart vb. (isterseniz genişletilir)

        Unknown = 0,
        Cash = 1,
        BankTransfer = 2,
        Pos = 3,
        Cheque = 4,
        Other = 9
    }
    public class Collection : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Ödemeyi yapan kiracı</summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>Ödeme hangi dükkân için alındı</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>O an geçerli sözleşme (versiyon)</summary>
        [Required]
        public Guid ContractId { get; set; }

        /// <summary>Ödeme tarihi (fiili tahsilat günü)</summary>
        [Required]
        public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        public bool IsApproved { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountReceived { get; set; }

        /// <summary>Ödeme yöntemi</summary>
        [Required]
        public PaymentMethod Method { get; set; }

        /// <summary>Banka/POS/makbuz referansı</summary>
        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        public Guid? MoneyEntryId { get; set; }

        /// <summary>Hedeflenen dönem</summary>
        [Range(1300, 1500)]
        public int? TargetYear { get; set; }

        [Range(1, 12)]
        public int? TargetMonth { get; set; } 

        /// <summary>Açıklama</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(TenantId))]
        public Tenant Tenant { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop Shop { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

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
        public ICollection<ReserveEntry> ReserveEntries { get; set; } = new List<ReserveEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<OwnerDistribution> OwnerDistributions { get; set; } = new List<OwnerDistribution>();

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

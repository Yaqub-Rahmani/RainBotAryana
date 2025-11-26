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
    //    2) Sözleşme(Contract)
    //Her sözleşme tek bir dükkân ve tek bir kiracıya bağlıdır.
    //Üç sabit kalem tutar içerir: Market Sahibi Payı, İmtiyaz Payı, Market Payı. (Elektrik değişkendir; sözleşmede tarifeyi/koşulu belirtiriz, tutarı her ay sayaçtan hesaplarız.)
    //İmza tarihi, başlangıç, bitiş, vade günü alanları vardır.
    //Sürümleme için Status = Active/Archived ve Version alanı bulunur(v1, v2…). Eski sözleşmeler Archived kalır, kayıtlar değişmez.
    //İş kuralı: Aynı dükkânda aynı anda tek aktif sözleşme(bunu EF konfig’de Unique Index ile koyacağız; ileride göstereceğim).
    public enum ContractStatus
    {
        Active = 1,
        Archived = 2
    }
    public class Contract : IEntity
    {

        [Key]
        public Guid Id { get; set; }

        /// <summary>Bağlı olduğu dükkân</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>Bağlı olduğu kiracı</summary>
        [Required]
        public Guid TenantId { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        public int? ArchivedByUserId { get; set; }
        public DateTime? ArchivedAtUtc { get; set; }

        /// <summary>İmza tarihi (sözleşmenin imzalandığı gün)</summary>
        [Required]
        public DateOnly SignedOn { get; set; }

        /// <summary>Geçerlilik başlangıcı</summary>
        [Required]
        public DateOnly StartDate { get; set; }

        /// <summary>Geçerlilik bitişi</summary>
        [Required]
        public DateOnly EndDate { get; set; }

        /// <summary>Her ay ödenmesi beklenen gün (1–31)</summary>
        [Range(1, 31)]
        public int DueDayOfMonth { get; set; }

        /// <summary>Market Sahibi Payı (sabit)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OwnerPortionAmount { get; set; }

        /// <summary>İmtiyaz Payı (sabit)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal FranchisePortionAmount { get; set; }

        /// <summary>Market Payı (sabit)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MarketPortionAmount { get; set; }

        /// <summary>Elektrik koşulu açıklaması (tarife/hesap yöntemi notu)</summary>
        [MaxLength(500)]
        public string? ElectricityNote { get; set; }

        /// <summary>Sözleşme başlarken elektrik sayacının endeksi</summary>
        [Precision(18, 3)]
        public decimal? InitialElectricityIndex { get; set; }

        [MaxLength(500)]
        public string? BusinessActivity { get; set; }

        /// <summary>Aktif/Arşiv (sürümleme)</summary>
        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        /// <summary>Versiyon numarası (v1, v2…)</summary>
        [MaxLength(20)]
        public string? Version { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }

        /// <summary>Arşiv/iptal gerekçesi (opsiyonel)</summary>
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

        // Yardımcı: Toplam sabit kira (elektrik hariç)
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

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

    public class Contract: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Bağlı olduğu dükkân</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>Bağlı olduğu kiracı</summary>
        [Required]
        public Guid TenantId { get; set; }

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

        /// <summary>Aktif/Arşiv (sürümleme)</summary>
        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        /// <summary>Versiyon numarası (v1, v2…)</summary>
        [MaxLength(20)]
        public string? Version { get; set; }

        /// <summary>Arşiv/iptal gerekçesi (opsiyonel)</summary>
        [MaxLength(500)]
        public string? ArchiveReason { get; set; }

        /// <summary>Oluşturma/Güncelleme izleri (opsiyonel)</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Navigation
        
        public Shop? Shop { get; set; }

        public Tenant? Tenant { get; set; }


        // Yardımcı: Toplam sabit kira (elektrik hariç)
        [NotMapped]
        public decimal TotalFixedRent => OwnerPortionAmount + FranchisePortionAmount + MarketPortionAmount;

        /// <summary>
        /// Sözleşme tarih tutarlılık kontrolü (isteğe bağlı domain metodu).
        /// Başlangıç <= Bitiş, İmza <= Bitiş vb. basit doğrulamaları üst katman çağırabilir.
        /// </summary>
        //public void ValidateDatesOrThrow()
        //{
        //    if (StartDate > EndDate)
        //        throw new InvalidOperationException("StartDate, EndDate’den büyük olamaz.");
        //    if (SignedOn > EndDate)
        //        throw new InvalidOperationException("SignedOn, EndDate’den büyük olamaz.");
        //}
    }
}

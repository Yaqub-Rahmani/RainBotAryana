using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Entities.Concrete
{
    //    Collection(Tahsilat)
    //Bir kiracıdan alınan tek bir ödemeyi temsil eder(tam/kısmi).
    //Hangi dükkan/sözleşme/dönem için olduğuna dair iz bırakmayı kolaylaştırır.
    //Dağıtım detayları CollectionAllocation satırlarında tutulur(business öncelik sırasına göre).

    public enum PaymentMethod
    {
        Cash = 1,        // Nakit
        BankTransfer = 2,// Havale/EFT
        Pos = 3          // POS/kart vb. (isterseniz genişletilir)
    }

    public class Collection: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Ödemeyi yapan kiracı</summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>Ödeme hangi dükkân için alındı</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>O an geçerli sözleşme (versiyon). Ayırt edilebilirlik için tutarız.</summary>
        [Required]
        public Guid ContractId { get; set; }

        /// <summary>Ödeme tarihi (fiili tahsilat günü)</summary>
        [Required]
        public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Toplam alınan tutar (kuruş duyarlılığı)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountReceived { get; set; }

        /// <summary>Ödeme yöntemi (nakit/havale/pos)</summary>
        [Required]
        public PaymentMethod Method { get; set; }

        /// <summary>Banka dekont no / POS slip no / makbuz no (opsiyonel)</summary>
        [MaxLength(100)]
        public string? ReferenceNo { get; set; }

        /// <summary>Bu ödemenin kapatmayı hedeflediği dönem (ör. geçmiş ay borcu)</summary>
        public int? TargetYear { get; set; }   // örn. 2025
        public int? TargetMonth { get; set; }  // 1–12

        /// <summary>Açıklama/not</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Nav
        public Tenant? Tenant { get; set; }
        public Shop? Shop { get; set; }
        public Contract? Contract { get; set; }

        public ICollection<CollectionAllocation> Allocations { get; set; } = new List<CollectionAllocation>();

    }
}

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
    //ReserveEntry(Rezerv Hareketi)
    //Her bir giriş/çıkış satırı.
    //Kaynağına referans verebilmek için opsiyonel bağlantılar:
    //Elektrik rezervine giriş = genelde CollectionAllocation (Electricity) üzerinden,
    //Market rezervine giriş = CollectionAllocation(Market),
    //Owner rezervine giriş = CollectionAllocation(Owner) iç dağıtımından,
    //Çıkış = bakım / onarım gideri vb. (serbest açıklama + opsiyonel belge no).

    public class ReserveEntry : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ReserveAccountId { get; set; }

        /// <summary>Giriş mi çıkış mı?</summary>
        [Required]
        public BalanceDirection Direction { get; set; }

        /// <summary>Tutar (2 ondalık)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public Guid? MoneyEntryId { get; set; }


        /// <summary>İşlem zamanı</summary>
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
        // --- Opsiyonel iz bağlantıları (nereden geldi/nereye gitti?) ---
        public Guid? CollectionId { get; set; }            // Tahsilatın kendisi
        public Guid? CollectionAllocationId { get; set; }  // Tahsilat dağıtım satırı
        public Guid? MeterReadingId { get; set; }          // Elektrik rezervine girişte ilişki
        public Guid? ContractId { get; set; }              // Bağlamı güçlendirmek için

        // Harcama belgesi/numarası (çıkışlarda)
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

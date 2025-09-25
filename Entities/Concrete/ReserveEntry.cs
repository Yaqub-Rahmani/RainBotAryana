using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using System.Runtime.ConstrainedExecution;

namespace Entities.Concrete
{
    //    ReserveEntry(Rezerv Hareketi)
    //Her bir giriş/çıkış satırı.
    //Kaynağına referans verebilmek için opsiyonel bağlantılar:
    //Elektrik rezervine giriş = genelde CollectionAllocation (Electricity) üzerinden,
    //Market rezervine giriş = CollectionAllocation(Market),
    //Owner rezervine giriş = CollectionAllocation(Owner) iç dağıtımından,
    //Çıkış = bakım / onarım gideri vb. (serbest açıklama + opsiyonel belge no).
    public enum ReserveEntryDirection
    {
        In = 1,   // Rezerv Girişi
        Out = 2   // Rezerv Çıkışı (harcama/aktarım)
    }

    public class ReserveEntry: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ReserveAccountId { get; set; }

        /// <summary>Giriş mi çıkış mı?</summary>
        [Required]
        public ReserveEntryDirection Direction { get; set; }

        /// <summary>Tutar (2 ondalık)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>İşlem zamanı</summary>
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Açıklama (örn. 'Elektrik %10 rezerv girişi', 'Bakım onarım faturası #123')</summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        // --- Opsiyonel iz bağlantıları (nereden geldi/nereye gitti?) ---
        public Guid? CollectionId { get; set; }            // Tahsilatın kendisi
        public Guid? CollectionAllocationId { get; set; }  // Tahsilat dağıtım satırı (en çok bu kullanılır)
        public Guid? MeterReadingId { get; set; }          // Elektrik rezervine girişte ilişki kurmak için
        public Guid? ContractId { get; set; }              // Bağlamı güçlendirmek için

        // Harcama belgesi/numarası (çıkışlarda)
        [MaxLength(100)]
        public string? ExternalDocNo { get; set; }

        // Nav
        public ReserveAccount? ReserveAccount { get; set; }
        public Collection? Collection { get; set; }
        public CollectionAllocation? CollectionAllocation { get; set; }
        public MeterReading? MeterReading { get; set; }
        public Contract? Contract { get; set; }

    }
}

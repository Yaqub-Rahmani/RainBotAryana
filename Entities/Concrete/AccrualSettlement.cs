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
    //    Tahsilatın Borçla Eşleşmesi – AccrualSettlement
    //Bu tablo, bir CollectionAllocation satırının hangi borcu(Accrual) ne kadar kapattığını kayıtlar.
    //Böylece:
    //“Geçen ayın borcu bu ay ödendi” senaryosu net görünür,
    //Kısmi kapanışlar detaylı izlenir,
    //Raporlamada “hangi tahsilat hangi borcu kapattı?” sorusu kolay cevaplanır.
    public class AccrualSettlement: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Hangi dağıtım satırından geldi? (Elektrik/Market/Owner/Franchise gibi)</summary>
        [Required]
        public Guid CollectionAllocationId { get; set; }

        /// <summary>Hangi borcu kapattı?</summary>
        [Required]
        public Guid AccrualId { get; set; }

        /// <summary>Bu eşleşmede kapatılan tutar</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Eşleştirme zamanı</summary>
        public DateTime SettledAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Opsiyonel not</summary>
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Nav
        public CollectionAllocation? CollectionAllocation { get; set; }
        public Accrual? Accrual { get; set; }
    }
}

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
    //    CollectionAllocation(Tahsilat Dağıtımı – satırlar)
    //Bir tahsilatın hangi kaleme ne kadar aktarıldığını tutar.
    //Elektrik satırı, gerekiyorsa ilgili sayaç okumasına bağlanabilir.

    public enum AllocationKind
    {
        Electricity = 1, // Elektrik borcu (aylık sayaçtan)
        Market = 2,      // Market payı
        Owner = 3,       // Market sahibi payı
        Franchise = 4    // İmtiyaz payı
    }

    public class CollectionAllocation : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Bağlı olduğu tahsilat</summary>
        [Required]
        public Guid CollectionId { get; set; }

        /// <summary>Dağıtılan kalem türü (Elektrik/Market/Owner/Franchise)</summary>
        [Required]
        public AllocationKind Kind { get; set; }

        /// <summary>Bu satırda dağıtılan tutar</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>İlgili sözleşme/dükkan/kiracı/dönem izini kolaylaştırmak için opsiyonel bağlar</summary>
        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? TenantId { get; set; }

        /// <summary>Elektrik satırıysa ilgili ayın sayaç okumasına bağlanabilir</summary>
        public Guid? MeterReadingId { get; set; }

        /// <summary>Bu dağıtımın hangi dönemi kapattığını belirtmek istersen</summary>
        public int? PeriodYear { get; set; }   // örn. 2025
        public int? PeriodMonth { get; set; }  // 1–12

        /// <summary>Not/açıklama</summary>
        [MaxLength(300)]
        public string? Notes { get; set; }

        // Nav
        public Collection? Collection { get; set; }
        public Contract? Contract { get; set; }
        public Shop? Shop { get; set; }
        public Tenant? Tenant { get; set; }
        public MeterReading? MeterReading { get; set; }

    }
}

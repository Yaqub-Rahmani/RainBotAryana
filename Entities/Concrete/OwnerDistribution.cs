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
    //    OwnerDistribution(Paket dağıtım olayı – anlık “snapshot”)
    //Market sahibi payı için tek bir dağıtım çalıştırması.O an kullanılan yüzdeleri ve tutarları snapshot olarak saklarız ki,
    //ileride oranlar değişse bile bu kayıt sabit kalsın.
    public class OwnerDistribution : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Kaynağına iz için: ilgili sözleşme ve (varsa) tahsilat/dağıtım satırı</summary>
        public Guid? ContractId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? CollectionAllocationId { get; set; } // Kind=Owner satırı genelde

        /// <summary>Dağıtıma giren toplam market sahibi payı</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OwnerAmountInput { get; set; }

        /// <summary>O an kullanılan oranlar (snapshot) – business belirler</summary>
        [Column(TypeName = "decimal(5,2)")] public decimal RateTax { get; set; }      // örn 20.00
        [Column(TypeName = "decimal(5,2)")] public decimal RateZakat { get; set; }    // 2.50
        [Column(TypeName = "decimal(5,2)")] public decimal RateCharity { get; set; }  // 1.50
        [Column(TypeName = "decimal(5,2)")] public decimal RateOwnerReserve { get; set; } // 16.00
        [Column(TypeName = "decimal(5,2)")] public decimal RateFamilyPool { get; set; }   // 60.00

        /// <summary>Çıkan tutarlar (snapshot)</summary>
        [Column(TypeName = "decimal(18,2)")] public decimal AmountTax { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal AmountZakat { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal AmountCharity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal AmountOwnerReserve { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal AmountFamilyPool { get; set; }

        /// <summary>İşlem zamanı</summary>
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Not/Açıklama</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}

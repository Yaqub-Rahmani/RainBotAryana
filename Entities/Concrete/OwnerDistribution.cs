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
    //OwnerDistribution(Paket dağıtım olayı – anlık “snapshot”)
    //Market sahibi payı için tek bir dağıtım çalıştırması.O an kullanılan yüzdeleri ve tutarları snapshot olarak saklarız ki,
    //ileride oranlar değişse bile bu kayıt sabit kalsın.
    // OwnerDistribution (Paket dağıtım olayı – anlık “snapshot”)
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
        [Column(TypeName = "decimal(5,2)")] public decimal RateTax { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateZakat { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateCharity { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateOwnerReserve { get; set; }
        [Column(TypeName = "decimal(5,2)")] public decimal RateFamilyPool { get; set; }

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

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract? Contract { get; set; }

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
        public ICollection<FamilyBalanceEntry> FamilyBalanceEntries { get; set; } = new List<FamilyBalanceEntry>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<FamilyPayout> FamilyPayouts { get; set; } = new List<FamilyPayout>();

        [JsonIgnore]
        [ValidateNever]
        public ICollection<OwnerDistributionLine> OwnerDistributionLines { get; set; } = new List<OwnerDistributionLine>();

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

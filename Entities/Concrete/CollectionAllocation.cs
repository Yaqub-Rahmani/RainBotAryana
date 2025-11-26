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

            public int CreatedByUserId { get; set; }
            public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

            /// <summary>Bu dağıtımın hangi dönemi kapattığını belirtmek istersen</summary>

            [Range(1300, 1500)]
            public int? PeriodYear { get; set; }
            [Range(1, 12)]
            public int? PeriodMonth { get; set; }  // 1–12

            /// <summary>Not/açıklama</summary>
            [MaxLength(300)]
            public string? Notes { get; set; }

            // Navigation
            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(CollectionId))]
            public Collection Collection { get; set; } = null!;

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(ContractId))]
            public Contract? Contract { get; set; }

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(ShopId))]
            public Shop? Shop { get; set; }

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(TenantId))]
            public Tenant? Tenant { get; set; }

            [JsonIgnore]
            [ValidateNever]
            [ForeignKey(nameof(MeterReadingId))]
            public MeterReading? MeterReading { get; set; }

            [JsonIgnore]
            [ValidateNever]
            public ICollection<AccrualSettlement> AccrualSettlements { get; set; } = new List<AccrualSettlement>();

            [JsonIgnore]
            [ValidateNever]
            public ICollection<FranchiseBalanceEntry> FranchiseBalanceEntries { get; set; } = new List<FranchiseBalanceEntry>();

            [JsonIgnore]
            [ValidateNever]
            public ICollection<OwnerDistribution> OwnerDistributions { get; set; } = new List<OwnerDistribution>();

            [JsonIgnore]
            [ValidateNever]
            public ICollection<ReserveEntry> ReserveEntries { get; set; } = new List<ReserveEntry>();
        }
}

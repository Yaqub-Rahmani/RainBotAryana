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

    //    tahakkuk/borç tarafı ile devam ediyoruz.Bu parçada iki yalın entity ekliyoruz:
    //Accrual → her ay oluşan borç kalemi(Owner/Franchise/Market sabitleri + Electricity sayaçtan),
    //AccrualSettlement → tahsilat dağıtımının hangi borcu kapattığını kayıtlar(kısmi/çoklu kapanış destekli).
    //Not: Hesaplama ve öncelik sırası(Elektrik → Market → Owner → Franchise) business katmanında kalacak; entity’ler yalnızca kayıt/izleme amacıyla yalın tutuldu.
    public enum AccrualKind
    {
        Owner = 1,       // Market sahibi payı (sabit)
        Franchise = 2,   // İmtiyaz payı (sabit)
        Market = 3,      // Market payı (sabit)
        Electricity = 4  // Elektrik (sayaçtan, %10 rezerv dahil faturalandırılan toplam)
    }

    public enum AccrualStatus
    {
        Open = 1,            // Tamamı açık borç
        PartiallySettled = 2,// Kısmi kapandı
        Settled = 3          // Tam kapandı
    }

    public class Accrual: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Dükkân</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>Kiracı</summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>Kaynağı olan sözleşme (sabit kalemler için zorunlu, elektrikte de bağ tutmak faydalı)</summary>
        [Required]
        public Guid ContractId { get; set; }

        /// <summary>Elektrik borcuysa ilgili sayaç okuması</summary>
        public Guid? MeterReadingId { get; set; }

        /// <summary>Kalem türü (Owner/Franchise/Market/Electricity)</summary>
        [Required]
        public AccrualKind Kind { get; set; }

        /// <summary>Borç hangi döneme ait? (YYYY, 1–12)</summary>
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>Oluşturulma (tahakkuk) tarihi</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Durum (Open/PartiallySettled/Settled)</summary>
        public AccrualStatus Status { get; set; } = AccrualStatus.Open;

        /// <summary>Toplam borç tutarı (sabit kalemlerde sözleşmeden, elektrikte sayaçtan; elektrik için %10 rezerv dahil faturalandırılan toplam)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Kalan borç (business tahsilat yaptıkça düşer)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingAmount { get; set; }

        /// <summary>Opsiyonel not</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Nav
        public Shop? Shop { get; set; }
        public Tenant? Tenant { get; set; }
        public Contract? Contract { get; set; }
        public MeterReading? MeterReading { get; set; }

        /// <summary>Basit tutarlılık yardımcıları</summary>
        public void InitializeOutstanding() => OutstandingAmount = Amount;

        public void ApplySettlement(decimal amount)
        {
            if (amount < 0) throw new InvalidOperationException("Settlement amount negative olamaz.");
            OutstandingAmount = Math.Round(Math.Max(0m, OutstandingAmount - amount), 2, MidpointRounding.AwayFromZero);
            Status = OutstandingAmount == 0 ? AccrualStatus.Settled : AccrualStatus.PartiallySettled;
        }
    }
}

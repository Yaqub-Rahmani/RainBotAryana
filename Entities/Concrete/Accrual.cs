using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Text.Json.Serialization;

namespace Entities.Concrete
{

    //    tahakkuk/borç tarafı ile devam ediyoruz.Bu parçada iki yalın entity ekliyoruz:
    //Accrual → her ay oluşan borç kalemi(Owner/Franchise/Market sabitleri + Electricity sayaçtan),
    //AccrualSettlement → tahsilat dağıtımının hangi borcu kapattığını kayıtlar(kısmi/çoklu kapanış destekli).
    //Not: Hesaplama ve öncelik sırası(Elektrik → Market → Owner → Franchise) business katmanında kalacak; entity’ler yalnızca kayıt/izleme amacıyla yalın tutuldu.

    public enum AccrualKind
    {
        Owner = 1,
        Franchise = 2,
        Market = 3,
        Electricity = 4
    }

    public enum AccrualStatus
    {
        Open = 1,
        PartiallySettled = 2,
        Settled = 3
    }

    public class Accrual : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Dükkân</summary>
        [Required]
        public Guid ShopId { get; set; }

        /// <summary>Kiracı</summary>
        [Required]
        public Guid TenantId { get; set; }

        /// <summary>Kaynağı olan sözleşme</summary>
        [Required]
        public Guid ContractId { get; set; }

        /// <summary>Elektrik borcuysa ilgili sayaç okuması</summary>
        public Guid? MeterReadingId { get; set; }

        /// <summary>Kalem türü (Owner/Franchise/Market/Electricity)</summary>
        [Required]
        public AccrualKind Kind { get; set; }

        /// <summary>Borç hangi döneme ait? (YYYY, 1–12)</summary>
        [Range(1300, 1500)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>Oluşturulma (tahakkuk) tarihi</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>Durum</summary>
        public AccrualStatus Status { get; set; } = AccrualStatus.Open;

        /// <summary>Toplam borç tutarı</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>Kalan borç</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingAmount { get; set; }

        /// <summary>Opsiyonel not</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ShopId))]
        public Shop Shop { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(TenantId))]
        public Tenant Tenant { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(ContractId))]
        public Contract Contract { get; set; } = null!;

        [JsonIgnore]
        [ValidateNever]
        [ForeignKey(nameof(MeterReadingId))]
        public MeterReading? MeterReading { get; set; }

        [JsonIgnore]
        [ValidateNever]
        public ICollection<AccrualSettlement> AccrualSettlements { get; set; } = new List<AccrualSettlement>();

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

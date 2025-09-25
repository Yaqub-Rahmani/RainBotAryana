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
    //    (MeterReading) ile devam ediyorum.Bu entity, her dükkân için aylık tüketimi ve %10 rezerv hesabını standartlaştırır.
    //3) Elektrik – MeterReading
    public class MeterReading: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Dükkân (Shop) referansı</summary>
        [Required]
        public Guid ShopId { get; set; }

        // --- Dönem (Aylık Kayıt) ---
        /// <summary>Yıl (örn. 2025)</summary>
        [Range(2000, 2100)]
        public int Year { get; set; }

        /// <summary>Ay (1-12)</summary>
        [Range(1, 12)]
        public int Month { get; set; }

        /// <summary>Okumanın yapıldığı tarih (raporlama için)</summary>
        public DateOnly ReadingDate { get; set; }

        // --- Sayaç Endeksleri (kWh) ---
        /// <summary>Önceki endeks</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal PreviousIndex { get; set; }

        /// <summary>Şimdiki endeks</summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal CurrentIndex { get; set; }

        /// <summary>Tüketim (kWh) = CurrentIndex - PreviousIndex (sadece görüntü amaçlı)</summary>
        [NotMapped]
        public decimal ConsumptionKwh => Math.Max(0, CurrentIndex - PreviousIndex);

        // --- Fiyat & Tutarlar ---
        /// <summary>Birim fiyat (kWh başına)</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        /// <summary>Net tutar = ConsumptionKwh * UnitPrice (rezerv hariç)</summary>
        [NotMapped]
        public decimal NetAmount => Math.Round(ConsumptionKwh * UnitPrice, 2, MidpointRounding.AwayFromZero);

        /// <summary>Rezerv oranı (varsayılan %10)</summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal ReserveRate { get; set; } = 0.10m;

        /// <summary>Rezerv tutarı = NetAmount * ReserveRate</summary>
        [NotMapped]
        public decimal ReserveAmount => Math.Round(NetAmount * ReserveRate, 2, MidpointRounding.AwayFromZero);

        /// <summary>Faturalandırılacak toplam = NetAmount + ReserveAmount</summary>
        [NotMapped]
        public decimal TotalBilled => NetAmount + ReserveAmount;

        // --- Durum / Kanıt ---
        /// <summary>Opsiyonel açıklama/nota alanı</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>Ay kapandı mı (artık değiştirme yapma niyeti yok)</summary>
        public bool IsFinalized { get; set; }

        // Nav
        public Shop? Shop { get; set; }


        /// <summary>Basit alan doğrulaması</summary>
        //public void ValidateOrThrow()
        //{
        //    if (CurrentIndex < PreviousIndex)
        //        throw new InvalidOperationException("CurrentIndex, PreviousIndex’ten küçük olamaz.");
        //    if (UnitPrice < 0)
        //        throw new InvalidOperationException("UnitPrice negatif olamaz.");
        //    if (ReserveRate < 0)
        //        throw new InvalidOperationException("ReserveRate negatif olamaz.");
        //}
    }
}

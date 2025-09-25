using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    //    ReserveAccount(Rezerv Hesabı)
    //Üç ayrı hesap: Elektrik / Market / Market Sahibi.
    //İstersen ileride farklı rezerv türleri de ekleyebilirsin.

    public enum ReserveAccountType
    {
        Electricity = 1, // Elektrik Rezervesi
        Market = 2,      // Market Rezervesi
        Owner = 3        // Market Sahibi Rezervesi
    }

    public class ReserveAccount: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Elektrik, Market veya Owner rezervi</summary>
        [Required]
        public ReserveAccountType Type { get; set; }

        /// <summary>Görünen ad (opsiyonel, rapor başlığı için)</summary>
        [MaxLength(200)]
        public string? DisplayName { get; set; }

        /// <summary>Durum</summary>
        public bool IsActive { get; set; } = true;
    }
}

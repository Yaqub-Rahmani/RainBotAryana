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
    //Elektrik & Market Rezerv Oranları(tek oranlı politikalar)


    public enum SingleRateType
    {
        ElectricityReserve = 1, // Elektrik tahsilatına uygulanacak % (örn. 10.00)
        MarketReserve = 2       // Market payına uygulanacak % (örn. 10.00)
    }
    public class SingleRatePolicy: IEntity
    {
        [Key] 
        public Guid Id { get; set; }

        [Required] 
        public SingleRateType Type { get; set; }

        /// <summary>Yüzde değer (örn. 10.00 = %10)</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal RatePercent { get; set; }

        /// <summary>Geçerlilik başlangıcı</summary>
        [Required] 
        public DateOnly EffectiveFrom { get; set; }

        /// <summary>Geçerlilik bitişi (açık uçluysa null)</summary>
        public DateOnly? EffectiveTo { get; set; }

        [MaxLength(300)] 
        public string? Notes { get; set; }
    }
}

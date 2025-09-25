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
    //    OwnerDistributionLine(isteğe bağlı, kategori bazlı satır)
    //Eğer raporlamada daha ayrıntı istersen, yukarıdaki snapshot’a ek olarak her parça için satır açabilirsin.
    //(Opsiyonel—temel kullanımda OwnerDistribution yeterlidir.)

    public enum DistributionPart
    {
        Tax = 1,        // Maliye
        Zakat = 2,      // Zekat
        Charity = 3,    // Sadaka
        OwnerReserve = 4,// Market Sahibi Rezervesi
        FamilyPool = 5   // Aile havuzu
    }

    public class OwnerDistributionLine: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid OwnerDistributionId { get; set; }

        [Required]
        public DistributionPart Part { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RateUsed { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}

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
    public enum SingleRateType
    {
        ElectricityReserve = 1,
        MarketReserve = 2      
    }
    public class SingleRatePolicy: IEntity
    {
        [Key] 
        public Guid Id { get; set; }

        [Required] 
        public SingleRateType Type { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal RatePercent { get; set; }

        [Required] 
        public DateOnly CreatedAtUtc { get; set; }

        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

        [MaxLength(300)] 
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
    }
}

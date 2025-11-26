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
    public class FamilySharePolicy : IEntity
    {
        [Key] public Guid Id { get; set; }

        /// <summary>Ebeveyn toplam yüzdesi (örn. 8.00 = %8)</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal ParentPercent { get; set; } = 8.00m;

        /// <summary>Kalan kısım için ağırlıklar: kız=1, erkek=2 gibi</summary>
        [Column(TypeName = "decimal(6,3)")]
        public decimal FemaleWeight { get; set; } = 1.000m;

        [Column(TypeName = "decimal(6,3)")]
        public decimal MaleWeight { get; set; } = 2.000m;

        [MaxLength(300)] public string? Notes { get; set; }

        [Required]
        public DateOnly CreatedAtUtc { get; set; }

        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}

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
    //    FamilyPayout & FamilyPayoutLine(Aile havuzunun paylaştırılması)
    //Aile havuzundaki tutarın, İslami pay kuralına göre üyelere dağıtıldığı olay(ve satırları).
    //Business pay hesabını yapar; burada kayıt altına alınır.
    public class FamilyPayout: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        /// <summary>Hangi OwnerDistribution’a bağlı aile havuzu dağıtımı</summary>
        [Required]
        public Guid OwnerDistributionId { get; set; }

        /// <summary>Dağıtılan toplam aile havuzu</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalFamilyAmount { get; set; }

        /// <summary>O an kullanılan kural özeti (ebeveyn %8, kalan kız=1 erkek=2)</summary>
        [MaxLength(500)]
        public string? RuleSummary { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<FamilyPayoutLine> Lines { get; set; } = new List<FamilyPayoutLine>();
    }
}

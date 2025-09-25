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

    public class FamilyPayoutLine : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FamilyPayoutId { get; set; }

        [Required]
        public Guid FamilyMemberId { get; set; }

        /// <summary>Üyeye ayrılan tutar</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }

        // Nav (opsiyonel)

        public FamilyMember? FamilyMember { get; set; }

    }
}

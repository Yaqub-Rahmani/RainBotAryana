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
    //    FamilyBalanceEntry(Aile üyesi bakiyesi hareketi)
    //Aile havuzundan üyelere dağıtılan tutarlar giriş; üye parayı çektiğinde çıkış.
    public class FamilyBalanceEntry: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FamilyMemberId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; } // In / Out

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        // İz bağlantıları (opsiyonel)
        public Guid? FamilyPayoutId { get; set; } // Aile havuzu dağıtım olayı (In)
        public Guid? OwnerDistributionId { get; set; } // Kaynak
        public Guid? CollectionId { get; set; } // Eğer doğrudan tahsilattan aktarım senaryosu olursa
        public Guid? ContractId { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}

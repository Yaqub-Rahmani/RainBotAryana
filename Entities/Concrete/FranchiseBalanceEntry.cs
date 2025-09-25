using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using Core.Entities;

namespace Entities.Concrete
{
    //    FranchiseBalanceEntry(İmtiyaz sahibi bakiyesi hareketi)
    //İmtiyaz payı tahsil edildiğinde giriş, çekim/ödemede çıkış kaydı.
    //(Havale masrafı o hesaptan düşülecekse, aynı tabloda negatif/çıkış hareketi.)

    public enum BalanceDirection { In = 1, Out = 2 } // Bakiye hareketi yönü

    public class FranchiseBalanceEntry: IEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FranchiseOwnerId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; } // In / Out

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // In için +, Out için + (yön belirler)

        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

        // İz bağlantıları (opsiyonel, rapor kolaylığı)
        public Guid? CollectionId { get; set; }            // Tahsilat
        public Guid? CollectionAllocationId { get; set; }  // Kind=Franchise satırı
        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }
        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }

        [MaxLength(300)]
        public string? Notes { get; set; }
    }
}

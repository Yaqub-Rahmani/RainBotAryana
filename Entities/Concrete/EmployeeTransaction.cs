using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class EmployeeTransaction: IEntity
    {
        public Guid Id { get; set; }

        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public BalanceDirection Direction { get; set; } // In / Out

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // In için +, Out için + (yön belirler)
        public Guid? MoneyEntryId { get; set; }

        [Range(1300, 1500)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }
        public DateTime PaidAtUtc { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }

    }
}

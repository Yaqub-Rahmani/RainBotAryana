using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FranchiseOwner.entry
{
    public class FranchiseBalanceEntryCreateDto
    {
        public Guid Id { get; set; }

        public Guid FranchiseOwnerId { get; set; }

        public BalanceDirection Direction { get; set; }
        public decimal Amount { get; set; }

        public DateTime OccurredAtUtc { get; set; } 

        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string? PaymentReference { get; set; }

        public Guid? MoneyEntryId { get; set; }
    }
}

using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.TaxPaymentEntry
{
    public class TaxPaymentEntryDetailDto
    {
        public Guid Id { get; set; }
        public BalanceDirection Direction { get; set; }
        public decimal Amount { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
    }
}

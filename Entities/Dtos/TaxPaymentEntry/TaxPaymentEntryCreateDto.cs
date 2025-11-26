using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.TaxPaymentEntry
{
    public class TaxPaymentEntryCreateDto: IDto
    {
        public BalanceDirection Direction { get; set; } = BalanceDirection.Out;
        public decimal Amount { get; set; }
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }

        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }
    }
}

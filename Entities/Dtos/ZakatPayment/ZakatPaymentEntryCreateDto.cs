using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.ZakatPayment
{
    public class ZakatPaymentEntryCreateDto
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

    public class ZakatPaymentEntryUpdateDto : ZakatPaymentEntryCreateDto
    {
        public Guid Id { get; set; }
    }

    public class ZakatPaymentEntryDetailDto
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

    public class ZakatPaymentEntryFilterDto
    {
        public DateTime? FromOccurredAtUtc { get; set; }
        public DateTime? ToOccurredAtUtc { get; set; }
        public BalanceDirection? Direction { get; set; }
        public bool? ApprovedOnly { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }
    }
}

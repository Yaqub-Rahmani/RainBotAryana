using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.TaxPaymentEntry
{
    public class TaxPaymentEntryFilterDto
    {
        public DateTime? FromOccurredAtUtc { get; set; }
        public DateTime? ToOccurredAtUtc { get; set; }

        public DateTime? FromCreatedAtUtc { get; set; }
        public DateTime? ToCreatedAtUtc { get; set; }

        public BalanceDirection? Direction { get; set; }
        public bool? ApprovedOnly { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }

        public int? CreatedByUserId { get; set; }
        public int? ApprovedByUserId { get; set; }
    }
}

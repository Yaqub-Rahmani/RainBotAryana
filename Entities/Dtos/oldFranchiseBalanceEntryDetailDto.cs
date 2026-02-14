using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class oldFranchiseBalanceEntryDetailDto : IDto
    {
        public Guid Id { get; set; }

        // Owner
        public Guid FranchiseOwnerId { get; set; }
        public string FranchiseOwnerName { get; set; } = "";

        // Audit (filtre/iz)
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = "";
        public DateOnly CreatedAtUtc { get; set; }

        // Transaction
        public string Direction { get; set; } = "";      // "In"/"Out"
        public string DirectionLabel { get; set; } = ""; // "ورودی/خروجی"
        public string ReferenceNo { get; set; } = "";
        public decimal Amount { get; set; }
        public PaymentMethod paymentMethod { get; set; }


        // Time (tek kaynak: OccurredAtUtc)
        public DateTime OccurredAtUtc { get; set; }
        public string OccurredTime { get; set; }
        public string DayName { get; set; } = ""; // شنبه...

        // Period
        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }
        public string? PeriodLabel { get; set; } // "1404-04"

        // Source links (izlenebilirlik)
        public Guid? CollectionId { get; set; }
        public Guid? CollectionAllocationId { get; set; }
        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }

        public string? Notes { get; set; }
    }
}

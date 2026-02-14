using Core.Entities;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FamilyMember
{
    public class FamilyBalanceEntryDetailDto : IDto
    {
        public Guid Id { get; set; }

        // Member
        public Guid FamilyMemberId { get; set; }
        public string FamilyMemberName { get; set; } = "";

        // Audit (filtre/iz)
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = "";
        public DateOnly CreatedAtUtc { get; set; }

        // Transaction
        public string Direction { get; set; } = "";      // "In"/"Out"
        public string DirectionLabel { get; set; } = ""; // فارسی label
        public decimal Amount { get; set; }
        public PaymentMethod paymentMethod { get; set; } = PaymentMethod.Unknown;
        public string ReferenceNo { get; set; } = "";

        // Time
        public DateTime OccurredAtUtc { get; set; }
        public string OccurredTime { get; set; } = ""; // 03:25 PM
        public string DayName { get; set; } = "";      // شنبه...

        // Period
        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }
        public string? PeriodLabel { get; set; } // "1404-04"

        // Trace links
        public Guid? FamilyPayoutId { get; set; }
        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }

        public string? Notes { get; set; }
    }
}

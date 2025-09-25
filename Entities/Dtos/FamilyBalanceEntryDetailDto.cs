using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FamilyBalanceEntryDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid FamilyMemberId { get; set; }
        public string FamilyMemberName { get; set; } = "";

        public string Direction { get; set; } = ""; // "In" / "Out"
        public decimal Amount { get; set; }
        public DateTime OccurredAtUtc { get; set; }

        public Guid? FamilyPayoutId { get; set; }
        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }

        public string? Notes { get; set; }
    }
}

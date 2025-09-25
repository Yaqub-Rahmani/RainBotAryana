using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FranchiseBalanceEntryDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid FranchiseOwnerId { get; set; }
        public string FranchiseOwnerName { get; set; } = "";

        public string Direction { get; set; } = ""; // "In"/"Out"
        public decimal Amount { get; set; }
        public DateTime OccurredAtUtc { get; set; }

        public Guid? CollectionId { get; set; }
        public Guid? CollectionAllocationId { get; set; }
        public Guid? ContractId { get; set; }
        public Guid? ShopId { get; set; }
        public int? PeriodYear { get; set; }
        public int? PeriodMonth { get; set; }
        public string? Notes { get; set; }
    }
}

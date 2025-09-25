using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ReserveEntryDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid ReserveAccountId { get; set; }
        public string AccountTitle { get; set; } = "";
        public string AccountType { get; set; } = "";
        public string Direction { get; set; } = ""; // In/Out
        public decimal Amount { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string? Description { get; set; }
        public string? ExternalDocNo { get; set; }

        // İz bağlantıları
        public Guid? CollectionId { get; set; }
        public Guid? CollectionAllocationId { get; set; }
        public Guid? MeterReadingId { get; set; }
        public Guid? ContractId { get; set; }
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.AccrualSettlement
{
    public class AccrualSettlementDetailDto:IDto
    {
        public Guid Id { get; set; }
        public Guid CollectionAllocationId { get; set; }
        public Guid AccrualId { get; set; }
        public decimal Amount { get; set; }
        public int SettledByUserId { get; set; }
        public DateTime SettledAtUtc { get; set; }
        public string? Notes { get; set; }

        // Zengin alanlar
        public decimal AccrualAmount { get; set; }
        public decimal AccrualOutstanding { get; set; }
        public int AccrualYear { get; set; }
        public int AccrualMonth { get; set; }
    }
}

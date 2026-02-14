using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.AccrualSettlement
{
    public class AccrualSettlementDto
    {
        public Guid Id { get; set; }

        public Guid CollectionAllocationId { get; set; }
        public Guid AccrualId { get; set; }

        public decimal Amount { get; set; }              // AFN
        public decimal AfnPerUsdAtThatTime { get; set; } // Kullanılan kur
        public decimal UsdEquivalent { get; set; }       // Amount / AfnPerUsd

        public int SettledByUserId { get; set; }
        public DateTime SettledAtUtc { get; set; }
        public string? Notes { get; set; }
    }

    public class AccrualSettlementCreateDto
    {
        public Guid CollectionAllocationId { get; set; }
        public Guid AccrualId { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }


}

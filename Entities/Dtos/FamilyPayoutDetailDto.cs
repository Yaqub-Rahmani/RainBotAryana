using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FamilyPayoutDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid OwnerDistributionId { get; set; }

        public decimal TotalFamilyAmount { get; set; }
        public string? RuleSummary { get; set; }
        public DateTime OccurredAtUtc { get; set; }

        // İlişkisel özetler
        public int LineCount { get; set; }
        public decimal LinesTotal { get; set; } // satırların toplamı (ileri adımda FamilyPayoutLine’dan)
    }
}

using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FamilyPayoutLineDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid FamilyPayoutId { get; set; }
        public Guid FamilyMemberId { get; set; }
        public string FamilyMemberName { get; set; } = "";
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}

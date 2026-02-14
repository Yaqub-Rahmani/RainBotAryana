using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract
{
    public class FranchiseOwnerShareDto
    {
        public Guid FranchiseOwnerId { get; set; }
        public string FranchiseOwnerName { get; set; } = string.Empty;

        public decimal SharePercent { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        // Ekranda filtrelemek istersen kullanırsın
        public bool IsActiveOnEvalDate { get; set; }
    }
}

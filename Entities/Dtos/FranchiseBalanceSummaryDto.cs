using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FranchiseBalanceSummaryDto : IDto
    {
        public Guid FranchiseOwnerId { get; set; }
        public string FranchiseOwnerName { get; set; } = "";
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }
}

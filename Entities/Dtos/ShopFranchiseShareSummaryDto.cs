using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ShopFranchiseShareSummaryDto : IDto
    {
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = "";
        public DateOnly OnDate { get; set; }
        public decimal TotalPercent { get; set; } // o tarihteki toplam %
        public bool IsBalanced => Math.Round(TotalPercent, 2) == 100.00m;
    }
}

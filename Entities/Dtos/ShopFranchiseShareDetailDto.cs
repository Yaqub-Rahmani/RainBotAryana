using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ShopFranchiseShareDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = "";
        public Guid FranchiseOwnerId { get; set; }
        public string FranchiseOwnerName { get; set; } = "";
        public decimal SharePercent { get; set; }
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public string? Notes { get; set; }
        public bool IsActiveOn { get; set; } // sorgu gününe göre
    }
}

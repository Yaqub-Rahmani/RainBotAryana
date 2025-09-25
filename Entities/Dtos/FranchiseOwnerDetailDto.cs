using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FranchiseOwnerDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        // İlişki özetleri
        public int ShopShareCount { get; set; }
        public int ActiveShopShareCount { get; set; }

        // Bakiye özetleri (FranchiseBalanceEntry’den)
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }
}

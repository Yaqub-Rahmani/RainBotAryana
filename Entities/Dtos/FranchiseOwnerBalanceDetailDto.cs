using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FranchiseOwnerBalanceDetailDto: IDto
    {
        public Guid Id { get; set; }
        public Guid FranchiseOwnerId { get; set; }
        public decimal Balance { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public Guid UpdatedAtBy { get; set; }

        // İsteğe bağlı: Owner adı-kodu gibi özet alanlar eklenebilir
        public string? OwnerName { get; set; }
    }
}

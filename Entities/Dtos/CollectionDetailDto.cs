using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CollectionDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; } = "";

        public Guid ShopId { get; set; }
        public string ShopName { get; set; } = "";

        public Guid ContractId { get; set; }

        public DateTime PaidAtUtc { get; set; }
        public decimal AmountReceived { get; set; }
        public string Method { get; set; } = "";
        public string? ReferenceNo { get; set; }
        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
        public string? Notes { get; set; }

        // toplam kaç allocation bağlanmış?
        public int AllocationCount { get; set; }
    }
}

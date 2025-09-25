using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenantDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = "";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? TaxOrIdNumber { get; set; }
        public bool IsActive { get; set; }

        // Sözleşme özeti (tekil navigation’a uygun)
        public Guid? ContractId { get; set; }
        public DateOnly? ContractStart { get; set; }
        public DateOnly? ContractEnd { get; set; }
        public string? ContractVersion { get; set; }
        public string? ContractStatus { get; set; }
    }
}

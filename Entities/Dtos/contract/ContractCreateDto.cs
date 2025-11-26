using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.ValidationRules.FluentValidation.Contract
{
    public class ContractCreateDto: IDto
    {
        public Guid ContractId { get; set; }
        public Guid ShopId { get; set; }
        public Guid TenantId { get; set; }
        public DateOnly SignedOn { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int DueDayOfMonth { get; set; }
        public decimal? InitialElectricityIndex { get; set; }
        public decimal OwnerPortionAmount { get; set; }
        public decimal FranchisePortionAmount { get; set; }
        public decimal MarketPortionAmount { get; set; }
        public string? ElectricityNote { get; set; }
        public string? BusinessActivity { get; set; }
        public string? Version { get; set; }
        public string? Note { get; set; }
    }
}

using Core.Entities;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.contract.pages
{
    public sealed class ContractListPageDto : IDto
    {
        public PagedResult<ContractListItemDto> Page { get; set; } = default!;

        // opsiyonel özetler (liste üstünde KPI gibi)
        public int ActiveCount { get; set; }
        public int ArchivedCount { get; set; }

        public decimal TotalOwnerPortion { get; set; }
        public decimal TotalFranchisePortion { get; set; }
        public decimal TotalMarketPortion { get; set; }
        public decimal TotalFixedRent => TotalOwnerPortion + TotalFranchisePortion + TotalMarketPortion;
    }

    public sealed class ContractListQueryOptions : QueryOptions
    {
        public Guid? MarketId { get; set; }
        public Guid? FloorId { get; set; }
        public Guid? ShopId { get; set; }
        public Guid? TenantId { get; set; }

        public ContractStatus? Status { get; set; }  // Active/Archived

        public DateOnly? StartFrom { get; set; }
        public DateOnly? StartTo { get; set; }

        public DateOnly? EndFrom { get; set; }
        public DateOnly? EndTo { get; set; }

        public int? DueDay { get; set; }
    }
}

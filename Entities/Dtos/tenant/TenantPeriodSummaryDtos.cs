using Core.Utilities.Paging;
using Core.Utilities.Query;

namespace Entities.Dtos.tenant
{
    public class TenantPeriodSummaryRowDto
    {
        public Guid ShopId { get; set; }
        public Guid ContractId { get; set; }

        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string? UnitNumber { get; set; }

        public int Year { get; set; }
        public int Month { get; set; }

        public decimal TotalAccrued { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
    }

    public class TenantPeriodSummaryQueryOptions : QueryOptions
    {
        // QueryOptions zaten page/pageSize/sort/search/filters alıyor
        // Custom alan eklemek gerekmez; filtreleri CustomTableV2 gönderiyor.
    }

    public class TenantPeriodSummaryPageDto
    {
        public PagedResult<TenantPeriodSummaryRowDto> Page { get; set; } = default!;

        public decimal TotalAccrued { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
    }
}

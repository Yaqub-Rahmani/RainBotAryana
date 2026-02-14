using Core.Entities;
using Core.Utilities.Paging;
using Entities.Concrete;
using System;

namespace Entities.Dtos.FamilyMember
{
    public class FamilyMemberBalanceEntryListItemDto : IDto
    {
        public Guid Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public BalanceDirection Direction { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }

        public Guid? FamilyPayoutId { get; set; }
        public Guid? OwnerDistributionId { get; set; }
        public Guid? CollectionId { get; set; }
        public Guid? ContractId { get; set; }
    }

    public class FamilyMemberBalanceSummaryDto : IDto
    {
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }

    public class FamilyMemberBalanceEntryPageDto : IDto
    {
        public PagedResult<FamilyMemberBalanceEntryListItemDto> Page { get; set; } = new();
        public FamilyMemberBalanceSummaryDto Summary { get; set; } = new();
    }

    public class FamilyMemberBalanceEntryQueryOptions : IDto
    {
        // paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;

        // sort: "OccurredAtUtc:desc" etc. (Core.Query helper already expects QueryOptions.Sort format,
        // but for this screen we keep it simple: SortBy + SortDir)
        public string? SortBy { get; set; } = "OccurredAtUtc";
        public string? SortDir { get; set; } = "desc";

        // filters
        public BalanceDirection? Direction { get; set; }
        public DateTime? FromUtc { get; set; }
        public DateTime? ToUtc { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public string? Search { get; set; }
    }

    public class FamilyMemberFingerprintUpsertDto : IDto
    {
        public string TemplateBase64 { get; set; } = default!;
        public Guid? RelatedMediaAssetId { get; set; }
    }
}

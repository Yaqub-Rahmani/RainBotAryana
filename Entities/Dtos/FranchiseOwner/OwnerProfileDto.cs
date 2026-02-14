using Core.Entities;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FranchiseOwner
{
    public class OwnerProfileDto: IDto
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string FatherName { get; set; }
        public Gender Gender { get; set; }
        public string NationalId { get; set; }

        public string PhonePrimary { get; set; }
        public string PhoneSecondary { get; set; }

        public string Province { get; set; }
        public string District { get; set; }
        public string AddressLine { get; set; }
        public string AddressFull { get; set; }

        public string Description { get; set; }

        public decimal Balance { get; set; }

        // Media
        public string AvatarUrl { get; set; }
        public string IdFrontUrl { get; set; }
        public string IdBackUrl { get; set; }
        public string FingerBitmapUrl { get; set; }
        public string ContractUrl { get; set; }
    }

    public class FranchiseOwnerBalanceEntryListItemDto
    {
        public Guid Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }          // ISO/UTC
        public BalanceDirection Direction { get; set; }      // In / Out
        public decimal Amount { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public string? PaymentReference { get; set; }
        public string? Notes { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class FranchiseOwnerBalanceEntryQueryOptions : QueryOptions
    {
        public DateOnly? FromDate { get; set; }             // inclusive
        public DateOnly? ToDate { get; set; }               // inclusive (backend’de exclusive’a çevireceğiz)

        public BalanceDirection? Direction { get; set; }    // null => hepsi
        public PaymentMethod? PaymentMethod { get; set; }   // null => hepsi

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public string? Search { get; set; }                 // Notes / Reference içinde arama
    }

    public class FranchiseOwnerBalanceEntryPageDto
    {
        public PagedResult<FranchiseOwnerBalanceEntryListItemDto> Page { get; set; } = default!;

        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Net => TotalIn - TotalOut;
    }

    public class PagedWithSummaryDto<T>
    {
        public PagedResult<T> Page { get; set; } = default!;
        public BalanceSummaryDto Summary { get; set; } = new();
    }

    public class BalanceSummaryDto
    {
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }


}

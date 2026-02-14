using Core.Entities;
using Core.Utilities.Paging;

namespace Entities.Dtos.FamilyMember
{
    public class FamilyBalanceTotalsDto : IDto
    {
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;
    }

    public class FamilyBalanceEntryDetailsResponseDto : IDto
    {
        public PagedResult<FamilyBalanceEntryDetailDto> Page { get; set; } = new();
        public FamilyBalanceTotalsDto Summary { get; set; } = new();
    }
}

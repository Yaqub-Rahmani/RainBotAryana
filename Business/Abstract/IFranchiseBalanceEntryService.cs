using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.FranchiseOwner;
using Entities.Dtos.FranchiseOwner.entry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IFranchiseBalanceEntryService
    {
        IDataResult<PagedWithSummaryDto<oldFranchiseBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null);
        // Yazmalar
        IResult Add(FranchiseBalanceEntryCreateDto dto, int userId);

    }
}

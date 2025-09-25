using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IFranchiseBalanceEntryService
    {
        // Okumalar
        IDataResult<List<FranchiseBalanceEntry>> GetAll(Guid? ownerId = null);
        IDataResult<FranchiseBalanceEntry> GetById(Guid id);
        IDataResult<PagedResult<FranchiseBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null);
        IDataResult<FranchiseBalanceEntryDetailDto> GetDetailById(Guid id);

        // Özet
        IDataResult<List<FranchiseBalanceSummaryDto>> GetSummary(Guid? ownerId = null, DateTime? fromUtc = null, DateTime? toUtc = null);

        // Yazmalar
        IResult Add(FranchiseBalanceEntry entry);
        IResult Update(FranchiseBalanceEntry entry);
        IResult Delete(Guid id);
    }
}

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
    public interface IFamilyBalanceEntryService
    {
        // Okumalar
        IDataResult<List<FamilyBalanceEntry>> GetAll();
        IDataResult<FamilyBalanceEntry> GetById(Guid id);
        IDataResult<PagedResult<FamilyBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? familyMemberId = null);
        IDataResult<FamilyBalanceEntryDetailDto> GetDetailById(Guid id);

        // Özet / rapor
        IDataResult<List<FamilyBalanceSummaryDto>> GetSummary(Guid? familyMemberId = null, DateTime? fromUtc = null, DateTime? toUtc = null);

        // Yazmalar
        IResult Add(FamilyBalanceEntry entry);
        IResult Update(FamilyBalanceEntry entry);
        IResult Delete(Guid id);
    }
}

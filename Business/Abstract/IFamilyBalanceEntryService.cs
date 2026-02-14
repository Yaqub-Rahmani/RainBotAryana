using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.FamilyMember;
using Entities.Dtos.FranchiseOwner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IFamilyBalanceEntryService
    {
        IDataResult<List<FamilyBalanceEntry>> GetAll();
        IDataResult<FamilyBalanceEntry> GetById(Guid id);
        IDataResult<Entities.Dtos.FamilyMember.FamilyBalanceEntryDetailDto> GetDetailById(Guid id);

        // ✅ NEW (Controller bunu çağırıyor)        
        IDataResult<PagedWithSummaryDto<FamilyBalanceEntryDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null);

        IDataResult<List<FamilyBalanceSummaryDto>> GetSummary(Guid? familyMemberId = null, DateTime? fromUtc = null, DateTime? toUtc = null);

        IResult Add(FamilyBalanceEntry entry);
        IResult Update(FamilyBalanceEntry entry);
        IResult Delete(Guid id);
    }
}

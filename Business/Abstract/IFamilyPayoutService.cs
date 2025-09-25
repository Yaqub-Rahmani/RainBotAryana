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
    public interface IFamilyPayoutService
    {
        // Okumalar
        IDataResult<List<FamilyPayout>> GetAll();
        IDataResult<FamilyPayout> GetById(Guid id);
        IDataResult<PagedResult<FamilyPayoutDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<FamilyPayoutDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(FamilyPayout payout);
        IResult Update(FamilyPayout payout);
        IResult Delete(Guid id);
    }
}

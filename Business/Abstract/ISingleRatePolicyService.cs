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
    public interface ISingleRatePolicyService
    {
        // Okumalar
        IDataResult<List<SingleRatePolicy>> GetAll(SingleRateType? type = null);
        IDataResult<SingleRatePolicy> GetById(Guid id);
        IDataResult<PagedResult<SingleRatePolicyDetailDto>> GetDetails(QueryOptions opt, SingleRateType? type = null);
        IDataResult<SingleRatePolicyDetailDto> GetDetailById(Guid id);
        IDataResult<SingleRatePolicyDetailDto> GetActive(SingleRateType type, DateOnly onDate);

        // Yazmalar
        IResult Add(SingleRatePolicy policy);
        IResult Update(SingleRatePolicy policy);
        IResult Delete(Guid id);

        // Simülasyon
        IDataResult<object> Simulate(SingleRateType type, decimal baseAmount, DateOnly? onDate = null);
    }
}

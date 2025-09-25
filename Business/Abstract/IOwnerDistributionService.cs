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
    public interface IOwnerDistributionService
    {
        IDataResult<List<OwnerDistribution>> GetAll(Guid? contractId = null);
        IDataResult<OwnerDistribution> GetById(Guid id);
        IDataResult<PagedResult<OwnerDistributionDetailDto>> GetDetails(QueryOptions opt, Guid? contractId = null);
        IDataResult<OwnerDistributionDetailDto> GetDetailById(Guid id);

        IResult Add(OwnerDistribution distribution);
        IResult Update(OwnerDistribution distribution);
        IResult Delete(Guid id);
    }
}

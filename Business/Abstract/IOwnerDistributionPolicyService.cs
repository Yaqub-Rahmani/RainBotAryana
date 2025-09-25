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
    public interface IOwnerDistributionPolicyService
    {
        // Reads
        IDataResult<List<OwnerDistributionPolicy>> GetAll();
        IDataResult<OwnerDistributionPolicy> GetById(Guid id);
        IDataResult<PagedResult<OwnerDistributionPolicyDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<OwnerDistributionPolicyDetailDto> GetDetailById(Guid id);
        IDataResult<OwnerDistributionPolicyDetailDto> GetActiveForDate(DateOnly onDate);

        // Writes
        IResult Add(OwnerDistributionPolicy policy);
        IResult Update(OwnerDistributionPolicy policy);
        IResult Delete(Guid id);

        // Simulation
        IDataResult<object> Simulate(decimal ownerAmount, DateOnly? onDate = null);
    }
}

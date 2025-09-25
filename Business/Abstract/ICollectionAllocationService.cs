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
    public interface ICollectionAllocationService
    {
        IDataResult<List<CollectionAllocation>> GetAll();
        IDataResult<CollectionAllocation> GetById(Guid id);
        IDataResult<PagedResult<CollectionAllocationDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<CollectionAllocationDetailDto> GetDetailById(Guid id);

        IResult Add(CollectionAllocation allocation);
        IResult Update(CollectionAllocation allocation);
        IResult Delete(Guid id);
    }
}

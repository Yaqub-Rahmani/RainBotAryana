using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.CollectionAllocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICollectionAllocationService
    {
        IDataResult<List<CollectionAllocation>> GetAll(Guid? collectionId = null, Guid? contractId = null);
        IDataResult<CollectionAllocation> GetById(Guid id);
        IDataResult<CollectionAllocationDetailDto> GetDetail(Guid id);

        IResult Add(CollectionAllocationCreateDto dto, int userId);
        IResult Update(CollectionAllocationUpdateDto dto, int userId);
        IResult Delete(Guid id);
    }
}

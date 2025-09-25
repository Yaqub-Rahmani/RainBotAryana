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
    public interface ICollectionService
    {
        IDataResult<List<Collection>> GetAll();
        IDataResult<Collection> GetById(Guid id);
        IDataResult<PagedResult<CollectionDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<CollectionDetailDto> GetDetailById(Guid id);

        IResult Add(Collection collection);
        IResult Update(Collection collection);
        IResult Delete(Guid id);
    }
}

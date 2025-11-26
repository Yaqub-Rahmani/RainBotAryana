using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICollectionService
    {
        IDataResult<List<Collection>> GetAll(Guid? tenantId = null, Guid? shopId = null, DateTime? fromUtc = null, DateTime? toUtc = null);
        IDataResult<Collection> GetById(Guid id);
        IDataResult<CollectionDetailDto> GetDetail(Guid id);

        IResult Add(CollectionCreateDto dto, int userId);
        IResult Update(CollectionUpdateDto dto, int userId);
        IResult Delete(Guid id);
    }
}

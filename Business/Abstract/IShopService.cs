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
    public interface IShopService
    {
        IDataResult<List<Shop>> GetAll(bool? onlyActive = null, Guid? marketId = null, Guid? floorId = null);
        IDataResult<Shop> GetById(Guid id);
        IDataResult<PagedResult<ShopDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null, Guid? floorId = null, bool? onlyActive = null);
        IDataResult<ShopDetailDto> GetDetailById(Guid id);

        IResult Add(Shop entity);
        IResult Update(Shop entity);
        IResult Delete(Guid id);
    }
}

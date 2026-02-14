using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IShopService
    {
        IDataResult<Shop> GetById(Guid id);
        Task<IDataResult<List<ShopShareSummaryDto>>> GetShopsWithSharesAsync(Guid ownerId);
        IDataResult<List<ShopDto>> GetShopListView();
        Task<IDataResult<List<FranchiseOwnerShopsDto>>> GetFranchiseOwnerShops(Guid ownerId);
        Task<IDataResult<List<ShopPickerDto>>> GetAvailableShopsAsync();

        IResult Add(ShopCreateDto entityn, int userId);
        IResult Update(Shop entity);
        IResult Delete(Guid id);

        Task<IDataResult<PagedResult<ShopShareSummaryDto>>> GetMyShopsPagedAsync(Guid ownerId, QueryOptions opt);
        Task<IDataResult<PagedResult<ShopShareSummaryDto>>> GetAvailableShopsPagedAsync(Guid ownerId, QueryOptions opt);

    }
}

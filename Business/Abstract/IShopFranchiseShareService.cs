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
    public interface IShopFranchiseShareService
    {
        // Okumalar
        IDataResult<List<ShopFranchiseShare>> GetAll(Guid? shopId = null, Guid? franchiseOwnerId = null);
        IDataResult<ShopFranchiseShare> GetById(Guid id);
        IDataResult<PagedResult<ShopFranchiseShareDetailDto>> GetDetails(QueryOptions opt, Guid? shopId = null, Guid? franchiseOwnerId = null, DateOnly? on = null);
        IDataResult<ShopFranchiseShareDetailDto> GetDetailById(Guid id);
        IDataResult<List<ShopFranchiseShareDetailDto>> GetActiveOn(Guid shopId, DateOnly onDate);
        IDataResult<ShopFranchiseShareSummaryDto> GetSummary(Guid shopId, DateOnly onDate);

        // Yazmalar
        IResult Add(ShopFranchiseShare entity);
        IResult Update(ShopFranchiseShare entity);
        IResult Delete(Guid id);
    }
}

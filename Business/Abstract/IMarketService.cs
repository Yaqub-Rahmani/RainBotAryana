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
    public interface IMarketService
    {
        IDataResult<List<Market>> GetAll(bool? status = null);
        IDataResult<Market> GetById(Guid id);
        IDataResult<PagedResult<MarketDetailDto>> GetDetails(QueryOptions opt, bool? status = null);
        IDataResult<MarketDetailDto> GetDetailById(Guid id);

        IResult Add(Market market);
        IResult Update(Market market);
        IResult Delete(Guid id);

        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}

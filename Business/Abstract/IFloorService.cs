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
    public interface IFloorService
    {
        IDataResult<List<Floor>> GetAll(bool? onlyActive = null);
        IDataResult<Floor> GetById(Guid id);
        IDataResult<PagedResult<FloorDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null);
        IDataResult<FloorDetailDto> GetDetailById(Guid id);

        IResult Add(Floor floor);
        IResult Update(Floor floor);
        IResult Delete(Guid id);
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
